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

        private readonly IEnumerable<ISimpleFileCompiler> _compilers;

        public ContentCompiler(ICompilerHost host)
        {
            _host = host;

            _compilers = new ISimpleFileCompiler[]
                {
                    new FileConcatenationCompiler(this),
                    new MinifyingFileCompiler(),
                    new CoffeeScriptFileCompiler(),
                    new SassFileCompiler(),
                    new JavascriptPassthroughCompiler(),
                    new CssPassthroughCompiler()
                };

            this.Init();
        }

        public ContentCompiler(ICompilerHost host, IEnumerable<ISimpleFileCompiler> compilers)
        {
            _host = host;
            _compilers = compilers;

            this.Init();
        }

        public bool CanCompile(string filePath)
        {
            return _compilers.Any(x => filePath.EndsWith(x.OutputFileExtension) && x.FindInputFileGivenOutput(filePath) != null);
        }

        public CompilationResult GetCompiledContent(string filePath)
        {
            var compiler = this.GetMatchingCompiler(filePath);
            if (compiler == null)
            {
                return CompilationResult.Error;
            }

            var inputPhysicalFileName = compiler.FindInputFileGivenOutput(filePath);
            if (!File.Exists(inputPhysicalFileName))
            {
                return CompilationResult.Error;
            }

            var cacheKey = this.GetCacheKey(inputPhysicalFileName, compiler);
            return this._host.Cache.GetOrAdd(cacheKey, f => this.CompileContent(inputPhysicalFileName, compiler));
        }

        public string GetSourceFileNameFromRequestedFileName(string filePath)
        {
            var compiler = this.GetMatchingCompiler(filePath);
            if (compiler == null)
            {
                return string.Empty;
            }

            return compiler.FindInputFileGivenOutput(filePath);
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

            return new CompilationResult(true, compiler.ProcessFileContent(physicalFileName), fi.LastWriteTimeUtc);
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