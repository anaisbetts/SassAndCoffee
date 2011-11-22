namespace SassAndCoffee.Core.Caching
{
    using System;
    using System.Collections.Generic;

    public class InMemoryCache : ICompiledCache
    {
        readonly MemoizingMRUCache<string, CompilationResult> _cache;
        readonly Dictionary<string, Func<string, CompilationResult>> _delegateIndex = new Dictionary<string, Func<string, CompilationResult>>();

        public InMemoryCache()
        {
            _cache = new MemoizingMRUCache<string, CompilationResult>((file, _) => {
                Func<string, CompilationResult> compiler = null;
                lock (_delegateIndex) {
                    compiler = _delegateIndex[file];
                }

                return compiler(file);
            }, 50);
        }

        public CompilationResult GetOrAdd(string filename, Func<string, CompilationResult> compilationDelegate, string mimeType)
        {
            lock (_delegateIndex) {
                _delegateIndex[filename] = compilationDelegate;
            }

            return _cache.Get(filename);
        }

        public void Clear()
        {
            lock(_delegateIndex) { _delegateIndex.Clear(); }
            _cache.InvalidateAll();
        }
    }
}
