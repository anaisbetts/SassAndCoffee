namespace SassAndCoffee.Core {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Caches results content results using the provided cache implementation.
    /// </summary>
    public class CachingContentPipeline : IContentPipeline {
        private CacheInvalidationWatcher _watcher = new CacheInvalidationWatcher();
        private IContentPipeline _innerPipeline;
        private IContentCache _cache;

        private ReaderWriterLockSlim _cacheAccountingLock = new ReaderWriterLockSlim();
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

            _innerPipeline = new ContentPipeline(transformations);

            _watcher.Changed += new FileSystemEventHandler(OnInvalidationChangedCreatedRenamedOrDeleted);
            _watcher.Created += new FileSystemEventHandler(OnInvalidationChangedCreatedRenamedOrDeleted);
            _watcher.Deleted += new FileSystemEventHandler(OnInvalidationChangedCreatedRenamedOrDeleted);
            _watcher.Error += new ErrorEventHandler(OnInvalidationError);
            _watcher.Renamed += new RenamedEventHandler(OnInvalidationRenamed);
        }

        public ContentResult ProcessRequest(string physicalPath) {
            var resource = new FileInfo(physicalPath).FullName;
            ContentResult result = null;

            /* Cache implementations are not required to be thread safe for writes.
             * Reads while not writing are safe on a per-item level. Enforcing safe
             * access patterns here allows us to consolidate requests as well as
             * serialize access to the cache.
             */

            // Try to get the lock for this item
            ReaderWriterLockSlim cacheItemLock = GetCacheLockForResource(resource);

            try {
                _cacheAccountingLock.EnterReadLock();
                cacheItemLock.EnterReadLock();

                // NB: We don't cahe 404s since they're potentially unlimited and could become a DDOS vector
                if (!_cache.TryGet(resource, out result) || result == null) {
                    // Not found.  Enter in upgradable mode
                    try {
                        cacheItemLock.ExitReadLock();
                        cacheItemLock.EnterUpgradeableReadLock();

                        if (!_cache.TryGet(resource, out result) || result == null) {
                            // Still not found. Now we really have to make it.
                            try {
                                cacheItemLock.EnterWriteLock();

                                // TODO: Do deep caching of compiled dependencies (for javascript includes in combine files, etc)
                                result = _innerPipeline.ProcessRequest(resource);

                                if (result != null) {
                                    SaveCacheItem(resource, result);
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
                if (_cacheAccountingLock.IsReadLockHeld) _cacheAccountingLock.ExitReadLock();
                if (cacheItemLock.IsReadLockHeld) cacheItemLock.ExitReadLock();
            }

            // Prevent denial of service by only saving locks for resources that exist (finite)
            if (result == null) {
                ReaderWriterLockSlim toDispose = null;
                if (_cacheLocks.TryRemove(resource, out toDispose)) {
                    toDispose.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the cache lock for the resource.
        /// No locks required to call this.
        /// </summary>
        /// <param name="resource">The resource.</param>
        private ReaderWriterLockSlim GetCacheLockForResource(string resource) {
            ReaderWriterLockSlim cacheItemLock = null;
            do {
                if (!_cacheLocks.TryGetValue(resource, out cacheItemLock)) {
                    ReaderWriterLockSlim newLock = null;
                    try {
                        newLock = new ReaderWriterLockSlim();
                        if (_cacheLocks.TryAdd(resource, newLock)) {
                            cacheItemLock = newLock;
                        }
                    } finally {
                        if (cacheItemLock != newLock) {
                            newLock.Dispose();
                        }
                    }
                }
            } while (cacheItemLock == null);
            return cacheItemLock;
        }

        private void OnInvalidationError(object sender, ErrorEventArgs e) {
            // Interesting for debugging, no useful action to take though.  Maybe clear whole cache if this happens?
        }

        private void OnInvalidationRenamed(object sender, RenamedEventArgs e) {
            EvictCacheItemsByDependency(e.OldFullPath);
        }

        private void OnInvalidationChangedCreatedRenamedOrDeleted(object sender, FileSystemEventArgs e) {
            EvictCacheItemsByDependency(e.FullPath);
        }

        /// <summary>
        /// Saves the cache item.
        /// You MUST have a write lock on the cache item before calling this method.
        /// You MUST have a read lock on the cacheAccountingLock before calling this method.
        /// The method ALWAYS exits with all locks in their pre-call state.
        /// </summary>
        /// <param name="resource">The resource to cache.</param>
        /// <param name="result">The result to cache.</param>
        private void SaveCacheItem(string resource, ContentResult result) {
            if (!_cacheAccountingLock.IsReadLockHeld)
                throw new InvalidOperationException("You must hold a read lock on the cacheAccountingLock before calling this method.");

            try {
                _cacheAccountingLock.ExitReadLock();
                _cacheAccountingLock.EnterWriteLock();

                var cacheItem = new CacheItem(resource);
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
                _cache.Set(resource, result);

                _cacheAccountingLock.ExitWriteLock();
                _cacheAccountingLock.EnterReadLock();
            } finally {
                if (_cacheAccountingLock.IsWriteLockHeld) _cacheAccountingLock.ExitWriteLock();
                if (!_cacheAccountingLock.IsReadLockHeld) _cacheAccountingLock.EnterReadLock();
            }
        }

        private void EvictCacheItemsByDependency(string dependencyPath) {
            try {
                _cacheAccountingLock.EnterWriteLock();

                // Find CacheDependency
                CacheDependency dependency = null;
                if (!_cacheDependencies.TryGetValue(dependencyPath, out dependency))
                    return;

                // Invalidate items produced by the altered dependency
                foreach (var item in dependency.Produces.ToArray()) {
                    // Remove the item from its dependencies
                    foreach (var dep in item.Dependencies.ToArray()) {
                        dep.Produces.Remove(item);

                        // Check if we need to track the dependency at all anymore
                        if (dep.Produces.Count == 0) {
                            _cacheDependencies.Remove(dep.PhysicalPath);
                        }
                    }

                    // Clear dependency tracking data (new version might be different)
                    _cacheItems.Remove(item.PhysicalPath);

                    _cache.Invalidate(item.PhysicalPath);
                }
            } finally {
                if (_cacheAccountingLock.IsWriteLockHeld) _cacheAccountingLock.ExitWriteLock();
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
                if (_cacheLocks != null) {
                    foreach (var cacheLock in _cacheLocks) {
                        ReaderWriterLockSlim toDispose;
                        if (_cacheLocks.TryRemove(cacheLock.Key, out toDispose)) {
                            toDispose.Dispose();
                        }
                    }
                    _cacheLocks = null;
                }
                if (_cacheAccountingLock != null) {
                    _cacheAccountingLock.Dispose();
                    _cacheAccountingLock = null;
                }
            }
        }
    }
}
