namespace SassAndCoffee.Core {
    /// <summary>
    /// The interface implemented by all content cache implementations.
    /// </summary>
    public interface IContentCache {
        /// <summary>
        /// Tries to get a cached copy of the requested path.  Returns false if not found.
        /// Must be thread safe with concurrent gets.
        /// </summary>
        /// <param name="path">The path requested.</param>
        /// <param name="result">The cached result. Null is a valid value.</param>
        bool TryGet(string path, out ContentResult result);

        /// <summary>
        /// Sets the cached result for the specified path. May not be thread safe.
        /// </summary>
        /// <param name="path">The path requested.</param>
        /// <param name="result">The result for that path.</param>
        void Set(string path, ContentResult result);

        /// <summary>
        /// Invalidates the specified cached path. May not be thread safe.
        /// </summary>
        /// <param name="path">The cached path to invalidate.</param>
        void Invalidate(string path);

        /// <summary>
        /// Clears the cache. May not be thread safe.
        /// </summary>
        void Clear();

        /// <summary>
        /// Initializes the cache. May throw exceptions and perform IO.
        /// Must be called before attempting to use the cache.
        /// May not be thread safe.
        /// </summary>
        void Initialize();
    }
}
