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

        public ContentCompiler(ICompilerHost host, ICompiledCache cache, IEnumerable<ISimpleFileCompiler> compilers)
        {
            _host = host;
            _cache = cache;
            _compilers = compilers;

            this.Init();
        }

        public static ContentCompiler WithAllCompilers(ICompilerHost host, ICompiledCache cache)
        {
            return new ContentCompiler(
                host,
                cache,
                new ISimpleFileCompiler[]
                {
                    new FileConcatenationCompiler(),
                    new MinifyingFileCompiler(),
                    new CoffeeScriptFileCompiler(),
                    new SassFileCompiler(),
                    new JavascriptPassthroughCompiler(),
                });
        }

        public bool CanCompile(string requestedFileName)
        {
            return _compilers.Any(x => requestedFileName.EndsWith(x.OutputFileExtension) && x.FindInputFileGivenOutput(requestedFileName) != null);
        }

        public CompilationResult GetCompiledContent (string requestedFileName)
        {
            var compiler = this.GetMatchingCompiler(requestedFileName);
            if (compiler == null)
            {
                return new CompilationResult(false, string.Empty, string.Empty);
            }

            var sourceFileName = compiler.FindInputFileGivenOutput(requestedFileName);
            var physicalFileName = this._host.MapPath(sourceFileName);
            if (!File.Exists(physicalFileName))
            {
                return new CompilationResult(false, string.Empty, string.Empty);
            }

            var cacheKey = this.GetCacheKey(physicalFileName, compiler);
            return this._cache.GetOrAdd(cacheKey, f => this.CompileContent(physicalFileName, compiler));
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
            return new CompilationResult(true, compiler.ProcessFileContent(physicalFileName), compiler.OutputMimeType);
        }

        private void Init()
        {
            foreach (var simpleFileCompiler in _compilers)
            {
                simpleFileCompiler.Init(this._host);
            }
        }

        private ISimpleFileCompiler GetMatchingCompiler(string requestedFileName)
        {
            return _compilers.FirstOrDefault(x => requestedFileName.EndsWith(x.OutputFileExtension) && x.FindInputFileGivenOutput(requestedFileName) != null);
        }
    }
}