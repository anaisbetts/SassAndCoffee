namespace SassAndCoffee.Core {
    using System;

    public interface IPersistentMedium {
        /// <summary>
        /// Initializes this instance.  Must be called before using the cache.
        /// </summary>
        void Initialize();

        /// <summary>
        /// If available, returns the cached content for the requested resource.
        /// Must be thread safe per key.
        /// </summary>
        /// <param name="key">The unique key for the resource requested.</param>
        /// <returns>The cached result, or null on cache miss.</returns>
        CachedContentResult TryGet(string key);

        /// <summary>
        /// Sets the cached content for the specified key.
        /// Must be thread safe per key.
        /// </summary>
        /// <param name="key">The unique key for the resource requested.</param>
        /// <param name="result">The result to cache.</param>
        void Set(string key, CachedContentResult result);

        /// <summary>
        /// Removes the cached content for the specified resource.
        /// Must be thread safe per key.
        /// </summary>
        /// <param name="key">The unique key of the cached resource to remove.</param>
        void Remove(string key);
    }
}
