namespace SassAndCoffee.Core {
    /// <summary>
    /// The interface implemented by all content cache implementations.
    /// </summary>
    public interface IContentCache {
        /// <summary>
        /// If available, returns the cached content for the requested resource. Returns false if not found.
        /// Must be thread safe per resource.
        /// </summary>
        /// <param name="resource">The resource requested.</param>
        /// <param name="result">The cached result. If null when returning true, interpreted as "Not Found".</param>
        bool TryGet(string resource, out ContentResult result);

        /// <summary>
        /// Sets the cached content for the specified resource.
        /// Need not be thread safe.
        /// </summary>
        /// <param name="resource">The resource requested.</param>
        /// <param name="result">The content for that resource.</param>
        void Set(string resource, ContentResult result);

        /// <summary>
        /// Invalidates the cached content for the specified resource.
        /// Need not be thread safe.
        /// </summary>
        /// <param name="resource">The cached resource to invalidate.</param>
        void Invalidate(string resource);

        /// <summary>
        /// Clears the cache.
        /// Need not be thread safe.
        /// </summary>
        void Clear();

        /// <summary>
        /// Initializes the cache. May throw exceptions and perform IO.
        /// Must be called before attempting to use the cache.
        /// Need not be thread safe.
        /// </summary>
        void Initialize();
    }
}
