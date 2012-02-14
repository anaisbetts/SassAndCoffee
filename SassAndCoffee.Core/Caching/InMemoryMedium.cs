namespace SassAndCoffee.Core {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// An in-memory cache. Right now it's unbounded.
    /// In the future I'd like to track:
    ///   * Last Access Time
    ///   * Generation Time (cost)
    ///   * Number of accesses (usage)
    /// Then I can make smarter eviction decisions.
    /// </summary>
    public class InMemoryMedium : IPersistentMedium {
        private ConcurrentDictionary<string, CachedContentResult> _items;
        private IEqualityComparer<string> _comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryMedium"/> class.
        /// Defaults to StringComparer.OrdinalIgnoreCase for resource comparisons.
        /// </summary>
        public InMemoryMedium()
            : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryMedium"/> class.
        /// Defaults to StringComparer.OrdinalIgnoreCase for resource comparisons.
        /// </summary>
        /// <param name="pathComparer">The comparer to use for resources. Defaults to StringComparer.OrdinalIgnoreCase</param>
        public InMemoryMedium(IEqualityComparer<string> pathComparer) {
            _comparer = pathComparer == null ? StringComparer.OrdinalIgnoreCase : pathComparer;
        }

        /// <summary>
        /// Initializes this instance.  Must be called before using the cache.
        /// </summary>
        public void Initialize() {
            _items = new ConcurrentDictionary<string, CachedContentResult>(_comparer);
        }

        /// <summary>
        /// If available, returns the cached content for the requested resource.
        /// Must be thread safe per key.
        /// </summary>
        /// <param name="key">The unique key for the resource requested.</param>
        /// <returns>
        /// The cached result, or null on cache miss.
        /// </returns>
        public CachedContentResult TryGet(string key) {
            CachedContentResult result = null;
            if (_items.TryGetValue(key, out result)) {
                return result;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Sets the cached content for the specified key.
        /// Must be thread safe per key.
        /// </summary>
        /// <param name="key">The unique key for the resource requested.</param>
        /// <param name="result">The result to cache.</param>
        public void Set(string key, CachedContentResult result) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the cached content for the specified resource.
        /// Must be thread safe per key.
        /// </summary>
        /// <param name="key">The unique key of the cached resource to remove.</param>
        public void Remove(string key) {
            CachedContentResult result = null;
            _items.TryRemove(key, out result);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            /* Do Nothing */
        }
    }
}
