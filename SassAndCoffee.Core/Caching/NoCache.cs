namespace SassAndCoffee.Core {

    /// <summary>
    /// Doesn't cache.
    /// </summary>
    public class NoCache : IContentCache {
        /// <summary>
        /// If available, returns the cached content for the requested resource. Returns false if not found.
        /// Must be thread safe per resource.
        /// </summary>
        /// <param name="resource">The resource requested.</param>
        /// <param name="result">The cached result. If null when returning true, interpreted as "Not Found".</param>
        /// <returns></returns>
        public bool TryGet(string resource, out ContentResult result) {
            result = null;
            return false;
        }

        /// <summary>
        /// Sets the cached content for the specified resource.
        /// Need not be thread safe.
        /// </summary>
        /// <param name="resource">The resource requested.</param>
        /// <param name="result">The content for that resource.</param>
        public void Set(string resource, ContentResult result) {
            /* Do nothing */
        }

        /// <summary>
        /// Invalidates the cached content for the specified resource.
        /// Need not be thread safe.
        /// </summary>
        /// <param name="resource">The cached resource to invalidate.</param>
        public void Invalidate(string resource) {
            /* Do nothing */
        }

        /// <summary>
        /// Clears the cache.
        /// Need not be thread safe.
        /// </summary>
        public void Clear() {
            /* Do nothing */
        }

        /// <summary>
        /// Initializes the cache. May throw exceptions and perform IO.
        /// Must be called before attempting to use the cache.
        /// Need not be thread safe.
        /// </summary>
        public void Initialize() {
            /* Do nothing */
        }
    }
}
