namespace SassAndCoffee.Core.Caching
{
    using System;
    using System.Collections.Concurrent;

    public class InMemoryCache : ICompiledCache
    {
        private readonly ConcurrentDictionary<string, CompilationResult> _cache = new ConcurrentDictionary<string, CompilationResult>();

        public CompilationResult GetOrAdd(string filename, Func<string, CompilationResult> compilationDelegate)
        {
            return this._cache.GetOrAdd(filename, compilationDelegate);
        }

        public void Clear()
        {
            this._cache.Clear();
        }
    }
}