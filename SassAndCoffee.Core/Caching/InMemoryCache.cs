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
        /// Defaults to StringComparer.OrdinalIgnoreCase for path comparisons.
        /// </summary>
        public InMemoryCache()
            : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryCache"/> class.
        /// </summary>
        /// <param name="pathComparer">The path comparer to use.</param>
        public InMemoryCache(IEqualityComparer<string> pathComparer) {
            _comparer = pathComparer == null ? StringComparer.OrdinalIgnoreCase : pathComparer;
            _items = new ConcurrentDictionary<string, ContentResult>(_comparer);
        }

        /// <summary>
        /// Tries to get a cached copy of the requested path.  Returns false if not found.
        /// </summary>
        /// <param name="path">The path requested.</param>
        /// <param name="result">The cached result. Null is a valid value.</param>
        /// <returns></returns>
        public bool TryGet(string path, out ContentResult result) {
            return _items.TryGetValue(path, out result);
        }

        /// <summary>
        /// Sets the cached result for the specified path.
        /// </summary>
        /// <param name="path">The path requested.</param>
        /// <param name="result">The result for that path.</param>
        public void Set(string path, ContentResult result) {
            _items.AddOrUpdate(path, result, (key, currentValue) => result);
        }

        /// <summary>
        /// Invalidates the specified cached path.
        /// </summary>
        /// <param name="path">The cached path to invalidate.</param>
        public void Invalidate(string path) {
            ContentResult result = null;
            _items.TryRemove(path, out result);
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear() {
            _items.Clear();
        }

        /// <summary>
        /// Initializes the cache. May throw exceptions and perform IO.
        /// Must be called before attempting to use the cache.
        /// </summary>
        public void Initialize() {
            _items = new ConcurrentDictionary<string, ContentResult>(_comparer);
        }
    }
}
