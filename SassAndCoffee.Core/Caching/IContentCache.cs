namespace SassAndCoffee.Core {
    using System;

    /// <summary>
    /// The interface implemented by all content cache implementations.
    /// </summary>
    public interface IContentCache {
        /// <summary>
        /// If available, returns the cached content for the requested resource.
        /// If the item is not found and the generator function is not null, calls the
        /// generator to produce the value and adds it before returning it.
        /// Must be thread safe per resource.
        /// </summary>
        /// <param name="key">The unique key for the resource requested.</param>
        /// <param name="generator">A generator function used to produce the value if it's not found.</param>
        /// <returns>The cached result.</returns>
        ContentResult GetOrAdd(string key, Func<string, ContentResult> generator);
    }
}
