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

        public CompilationResult GetOrAdd(string filename, Func<string, CompilationResult> compilationDelegate, string mimeType)
        {
            var outputFileName = Path.Combine(this._basePath, filename);
            FileInfo fi;

            if (File.Exists(outputFileName))
            {
                fi = new FileInfo(outputFileName);
                return new CompilationResult(true, File.ReadAllText(outputFileName), mimeType, fi.LastWriteTimeUtc);
            }

            var result = compilationDelegate.Invoke(filename);
            try
            {
                File.WriteAllText(outputFileName, result.Contents);
                fi = new FileInfo(outputFileName);
                fi.LastWriteTimeUtc = result.SourceLastModifiedUtc;
            }
            catch (IOException)
            {
                // TODO - lock around this to prevent multiple requests screwing up, or just catch and return?
            }
            
            return result;
        }

        public void Clear()
        {

        }
    }
}