namespace SassAndCoffee.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using SassAndCoffee.Core.Caching;
    using SassAndCoffee.Core.Compilers;
    using SassAndCoffee.Core.Extensions;

    public class ContentCompiler : IContentCompiler
    {
        private readonly ICompilerHost _host;

        private readonly ICompiledCache _cache;

        private readonly IEnumerable<ISimpleFileCompiler> _compilers;

        public ContentCompiler(ICompilerHost host, ICompiledCache cache)
        {
            _host = host;
            _cache = cache;

            _compilers = new ISimpleFileCompiler[]
                {
                    new FileConcatenationCompiler(this),
                    new MinifyingFileCompiler(),
                    new CoffeeScriptFileCompiler(),
                    new SassFileCompiler(),
                    new JavascriptPassthroughCompiler(),
                };

            this.Init();
        }

        public ContentCompiler(ICompilerHost host, ICompiledCache cache, IEnumerable<ISimpleFileCompiler> compilers)
        {
            _host = host;
            _cache = cache;
            _compilers = compilers;

            this.Init();
        }

        public bool CanCompile(string requestedFileName)
        {
            var physicalFileName = this._host.MapPath(requestedFileName);
            return _compilers.Any(x => physicalFileName.EndsWith(x.OutputFileExtension) && x.FindInputFileGivenOutput(physicalFileName) != null);
        }

        public CompilationResult GetCompiledContent (string requestedFileName)
        {
            var sourceFileName = this._host.MapPath(requestedFileName);
            var compiler = this.GetMatchingCompiler(sourceFileName);
            if (compiler == null)
            {
                return CompilationResult.Error;
            }

            var physicalFileName = compiler.FindInputFileGivenOutput(sourceFileName);
            if (!File.Exists(physicalFileName))
            {
                return CompilationResult.Error;
            }

            var cacheKey = this.GetCacheKey(physicalFileName, compiler);
            return this._cache.GetOrAdd(cacheKey, f => this.CompileContent(physicalFileName, compiler), compiler.OutputMimeType);
        }

        public string GetSourceFileNameFromRequestedFileName(string requestedFileName)
        {
            var physicalFileName = this._host.MapPath(requestedFileName);
            var compiler = this.GetMatchingCompiler(physicalFileName);
            if (compiler == null)
            {
                return string.Empty;
            }

            return compiler.FindInputFileGivenOutput(requestedFileName);
        }

        public string GetOutputMimeType(string requestedFileName)
        {
            var physicalFileName = this._host.MapPath(requestedFileName);
            var compiler = this.GetMatchingCompiler(physicalFileName);
            if (compiler == null)
            {
                return "application/octet-stream";
            }

            return compiler.OutputMimeType;
        }

        private string GetCacheKey(string physicalFileName, ISimpleFileCompiler compiler)
        {
            var fi = new FileInfo(physicalFileName);
            var token = compiler.GetFileChangeToken(physicalFileName) ?? String.Empty;

            return String.Format("{0:yyyyMMddHHmmss}-{1}-{2}{3}",
                        fi.LastWriteTimeUtc, token,
                        Path.GetFileNameWithoutExtension(physicalFileName),
                        compiler.OutputFileExtension);
        }

        private CompilationResult CompileContent(string physicalFileName, ISimpleFileCompiler compiler)
        {
            var fi = new FileInfo(physicalFileName);

            return new CompilationResult(true, compiler.ProcessFileContent(physicalFileName), compiler.OutputMimeType, fi.LastWriteTimeUtc);
        }

        private void Init()
        {
            foreach (var simpleFileCompiler in _compilers)
            {
                simpleFileCompiler.Init(this._host);
            }
        }

        private ISimpleFileCompiler GetMatchingCompiler(string physicalFileName)
        {
            return _compilers.FirstOrDefault(x => physicalFileName.EndsWith(x.OutputFileExtension) && x.FindInputFileGivenOutput(physicalFileName) != null);
        }
    }
}