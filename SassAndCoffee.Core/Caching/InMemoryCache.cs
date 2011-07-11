namespace SassAndCoffee.Core.Caching
{
    using System;
    using System.Collections.Concurrent;

    public class InMemoryCache : ICompiledCache
    {
        readonly ConcurrentDictionary<string, CompilationResult> _cache = new ConcurrentDictionary<string, CompilationResult>();

        public CompilationResult GetOrAdd(string filename, Func<string, CompilationResult> compilationDelegate)
        {
            return _cache.GetOrAdd(filename, compilationDelegate);
        }

        public void Clear()
        {
            _cache.Clear();
        }
    }
}
