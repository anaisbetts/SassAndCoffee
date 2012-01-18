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
    public class InMemoryCache : IContentCache {
        private ConcurrentDictionary<string, ContentResult> _items;
        private IEqualityComparer<string> _comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryCache"/> class.
        /// Defaults to StringComparer.OrdinalIgnoreCase for resource comparisons.
        /// </summary>
        public InMemoryCache()
            : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryCache"/> class.
        /// Defaults to StringComparer.OrdinalIgnoreCase for resource comparisons.
        /// </summary>
        /// <param name="pathComparer">The comparer to use for resources. Defaults to StringComparer.OrdinalIgnoreCase</param>
        public InMemoryCache(IEqualityComparer<string> pathComparer) {
            _comparer = pathComparer == null ? StringComparer.OrdinalIgnoreCase : pathComparer;
        }

        /// <summary>
        /// If available, returns the cached content for the requested resource. Returns false if not found.
        /// Must be thread safe per resource.
        /// </summary>
        /// <param name="resource">The resource requested.</param>
        /// <param name="result">The cached result. If null when returning true, interpreted as "Not Found".</param>
        /// <returns></returns>
        public bool TryGet(string resource, out ContentResult result) {
            return _items.TryGetValue(resource, out result);
        }

        /// <summary>
        /// Sets the cached content for the specified resource.
        /// Need not be thread safe.
        /// </summary>
        /// <param name="resource">The resource requested.</param>
        /// <param name="result">The content for that resource.</param>
        public void Set(string resource, ContentResult result) {
            _items.AddOrUpdate(resource, result, (key, currentValue) => result);
        }

        /// <summary>
        /// Invalidates the cached content for the specified resource.
        /// Need not be thread safe.
        /// </summary>
        /// <param name="resource">The cached resource to invalidate.</param>
        public void Invalidate(string resource) {
            ContentResult result = null;
            _items.TryRemove(resource, out result);
        }

        /// <summary>
        /// Clears the cache.
        /// Need not be thread safe.
        /// </summary>
        public void Clear() {
            _items.Clear();
        }

        /// <summary>
        /// Initializes the cache. May throw exceptions and perform IO.
        /// Must be called before attempting to use the cache.
        /// Need not be thread safe.
        /// </summary>
        public void Initialize() {
            _items = new ConcurrentDictionary<string, ContentResult>(_comparer);
        }
    }
}
