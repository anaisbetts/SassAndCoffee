namespace SassAndCoffee.Core.Caching
{
    using System;
    using System.IO;

    public class FileCache : ICompiledCache
    {
        private readonly string _basePath;

        public FileCache(string basePath)
        {
            if (String.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
            {
                throw new ArgumentException("basePath");
            }

            _basePath = basePath;
        }

        public CompilationResult GetOrAdd(string filename, Func<string, CompilationResult> compilationDelegate)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}