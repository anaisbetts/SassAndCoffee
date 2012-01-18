namespace SassAndCoffee.Core {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// Caches results content results using the provided cache implementation.
    /// </summary>
    public class CachingContentPipeline : IContentPipeline {
        private CacheInvalidationWatcher _watcher = new CacheInvalidationWatcher();
        private Dictionary<Guid, string> _invalidationKeys = new Dictionary<Guid, string>();
        private IContentPipeline _innerPipeline;
        private IContentCache _cache;

        private object _cacheAccountingLock = new object();
        private Dictionary<string, CacheItem> _cacheItems =
            new Dictionary<string, CacheItem>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, CacheDependency> _cacheDependencies =
            new Dictionary<string, CacheDependency>(StringComparer.OrdinalIgnoreCase);

        private ConcurrentDictionary<string, ReaderWriterLockSlim> _cacheLocks =
            new ConcurrentDictionary<string, ReaderWriterLockSlim>(StringComparer.OrdinalIgnoreCase);

        public IList<IContentTransform> Transformations { get { return _innerPipeline.Transformations; } }

        public CachingContentPipeline(IContentCache cache, params IContentTransform[] transformations)
            : this(cache, (IEnumerable<IContentTransform>)transformations) { }

        public CachingContentPipeline(IContentCache cache, IEnumerable<IContentTransform> transformations) {
            _cache = cache;

            // TODO: Ditch this to support deep caching
            _innerPipeline = new ContentPipeline(transformations);

            _watcher.Changed += new FileSystemEventHandler(OnInvalidationChangedCreatedRenamedOrDeleted);
            _watcher.Created += new FileSystemEventHandler(OnInvalidationChangedCreatedRenamedOrDeleted);
            _watcher.Deleted += new FileSystemEventHandler(OnInvalidationChangedCreatedRenamedOrDeleted);
            _watcher.Error += new ErrorEventHandler(OnInvalidationError);
            _watcher.Renamed += new RenamedEventHandler(OnInvalidationRenamed);
        }

        public ContentResult ProcessRequest(string physicalPath) {
            ContentResult result = null;

            /* Cache implementations are not required to be thread safe for writes.
             * Reads while not writing are safe on a per-item level. Enforcing safe
             * access patterns here allows us to consolidate requests as well as
             * serialize access to the cache.
             */

            // Try to get the lock for this item
            ReaderWriterLockSlim cacheItemLock = null;
            do {
                if (!_cacheLocks.TryGetValue(physicalPath, out cacheItemLock)) {
                    ReaderWriterLockSlim newLock = null;
                    try {
                        newLock = new ReaderWriterLockSlim();
                        if (_cacheLocks.TryAdd(physicalPath, newLock)) {
                            cacheItemLock = newLock;
                        }
                    } finally {
                        if (cacheItemLock != newLock) {
                            newLock.Dispose();
                        }
                    }
                }
            }
            while (cacheItemLock == null);

            // We have a lock!
            try {
                cacheItemLock.EnterReadLock();

                // TODO: Cache 404s too.  Right now the null check deliberately disables this.
                if (!_cache.TryGet(physicalPath, out result) || result == null) {

                    // Enter in upgradable mode
                    try {
                        cacheItemLock.ExitReadLock();
                        cacheItemLock.EnterUpgradeableReadLock();

                        // TODO: Cache 404s too.  Right now the null check deliberately disables this.
                        if (!_cache.TryGet(physicalPath, out result) || result == null) {
                            // OK, now we really have to make it.
                            try {
                                cacheItemLock.EnterWriteLock();

                                // TODO: Do deep caching of compiled dependencies (for javascript includes, etc)
                                result = _innerPipeline.ProcessRequest(physicalPath);

                                // TODO: Cache 404s too.  This prevents that.
                                if (result != null) {
                                    SaveCacheItem(physicalPath, result);
                                }
                            } finally {
                                if (cacheItemLock.IsWriteLockHeld) cacheItemLock.ExitWriteLock();
                            }
                        }
                    } finally {
                        if (cacheItemLock.IsUpgradeableReadLockHeld) cacheItemLock.ExitUpgradeableReadLock();
                    }
                }
            } finally {
                if (cacheItemLock.IsReadLockHeld) cacheItemLock.ExitReadLock();
            }

            // Prevent denial of service by only saving locks for resources that exist (finite)
            if (result == null) {
                if (_cacheLocks.TryRemove(physicalPath, out cacheItemLock)) {
                    cacheItemLock.Dispose();
                } else {
                    throw new SynchronizationLockException("Nick can't write threading code. Report error 5885 on github issues for SassAndCoffee.");
                }
            }

            return result;
        }

        private void OnInvalidationError(object sender, ErrorEventArgs e) {
            var ex = e.GetException();
        }

        private void OnInvalidationRenamed(object sender, RenamedEventArgs e) {
            EvictCacheItemsByDependency(e.OldFullPath);
        }

        private void OnInvalidationChangedCreatedRenamedOrDeleted(object sender, FileSystemEventArgs e) {
            EvictCacheItemsByDependency(e.FullPath);
        }

        private void SaveCacheItem(string cacheItemPhysicalPath, ContentResult result) {
            lock (_cacheAccountingLock) {
                // Abort if already cached.  When I'm done this should never happen.
                if (_cacheItems.ContainsKey(cacheItemPhysicalPath))
                    return;

                var cacheItem = new CacheItem(cacheItemPhysicalPath);
                _cacheItems.Add(cacheItem.PhysicalPath, cacheItem);
                foreach (var requiredFile in result.CacheInvalidationFileList) {
                    CacheDependency dependency = null;
                    // Find existing
                    if (!_cacheDependencies.TryGetValue(requiredFile, out dependency)) {
                        // It's a new one!
                        dependency = new CacheDependency(requiredFile);
                        _cacheDependencies.Add(dependency.PhysicalPath, dependency);

                        // Configure change watching
                        var depInfo = new FileInfo(dependency.PhysicalPath);
                        string filter = "";
                        if (!string.IsNullOrWhiteSpace(depInfo.Extension)) {
                            filter = "*" + depInfo.Extension;
                        }
                        _watcher.Watch(depInfo.DirectoryName, filter);
                    }
                    // Add production link
                    dependency.Produces.UnionWith(new CacheItem[] { cacheItem });
                    // Add dependency link
                    cacheItem.Dependencies.UnionWith(new CacheDependency[] { dependency });
                }

                // TODO: Locking around cache access?
                _cache.Set(cacheItemPhysicalPath, result);
            }
        }

        private void EvictCacheItemsByDependency(string dependencyPath) {
            lock (_cacheAccountingLock) {
                // Find CacheDependency
                CacheDependency dependency = null;
                if (!_cacheDependencies.TryGetValue(dependencyPath, out dependency))
                    return;

                var cacheItemsToInvalidate = dependency.Produces;

                // Invalidate items produced by the altered dependency
                foreach (var item in cacheItemsToInvalidate) {
                    // Remove the item from its dependencies
                    foreach (var dep in item.Dependencies) {
                        dep.Produces.Remove(item);

                        // Check if we need to track the dependency at all anymore
                        if (dep.Produces.Count == 0) {
                            _cacheDependencies.Remove(dep.PhysicalPath);
                        }
                    }

                    // Clear dependency tracking data (new version might be different)
                    _cacheItems.Remove(item.PhysicalPath);

                    // TODO: Locking around cache access?
                    _cache.Invalidate(item.PhysicalPath);
                }
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_innerPipeline != null) {
                    _innerPipeline.Dispose();
                    _innerPipeline = null;
                }
                if (_watcher != null) {
                    _watcher.Dispose();
                    _watcher = null;
                }
            }
        }
    }
}
