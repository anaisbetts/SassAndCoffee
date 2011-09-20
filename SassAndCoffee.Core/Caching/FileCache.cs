﻿namespace SassAndCoffee.Core.Caching
{
    using System;
    using System.IO;

    public class FileCache : ICompiledCache
    {
        private readonly string _basePath;

        public FileCache(string basePath)
        {
            if (String.IsNullOrEmpty(basePath) || !Directory.Exists(basePath)) {
                throw new ArgumentException("basePath");
            }

            _basePath = basePath;
        }

        public CompilationResult GetOrAdd(string filename, Func<string, CompilationResult> compilationDelegate, string mimeType)
        {
            var outputFileName = Path.Combine(_basePath, filename);
						FileInfo fi = new FileInfo(outputFileName);

            if (fi.Exists) {
                return new CompilationResult(true, File.ReadAllText(outputFileName), mimeType, fi.LastWriteTimeUtc);
            }

            var result = compilationDelegate(filename);

            try {
                File.WriteAllText(outputFileName, result.Contents);

                // XXX: Is this needed?
                fi.Refresh();
                fi.LastWriteTimeUtc = result.SourceLastModifiedUtc;
            } catch (IOException) {
                // NB: If we get here, this means that two threads are trying to 
                // write the file concurrently - just let the other one win, we will
                // later try to serve up the static file anyways
                //
                // TODO: Verify this :)
            }
            
            return result;
        }

        public void Clear()
        {

        }
    }
}
