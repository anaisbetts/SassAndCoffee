namespace SassAndCoffee.Core {

    /// <summary>
    /// Doesn't cache.
    /// </summary>
    public class NoCache : IContentCache {
        /// <summary>
        /// Tries to get a cached copy of the requested path.  Returns null if not found.
        /// </summary>
        /// <param name="path">The path requested.</param>
        /// <param name="result">The cached result. Null is a valid value.</param>
        public bool TryGet(string path, out ContentResult result) {
            result = null;
            return false;
        }

        /// <summary>
        /// Sets the cached result for the specified path.
        /// </summary>
        /// <param name="path">The path requested.</param>
        /// <param name="result">The result for that path.</param>
        public void Set(string path, ContentResult result) {
            /* Do nothing */
        }

        /// <summary>
        /// Invalidates the specified cached path.
        /// </summary>
        /// <param name="path">The cached path to invalidate.</param>
        public void Invalidate(string path) {
            /* Do nothing */
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear() {
            /* Do nothing */
        }

        /// <summary>
        /// Initializes the cache. May throw exceptions and perform IO.
        /// Must be called before attempting to use the cache.
        /// </summary>
        public void Initialize() {
            /* Do nothing */
        }
    }
}
