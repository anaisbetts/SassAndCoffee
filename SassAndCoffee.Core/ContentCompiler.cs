namespace SassAndCoffee.Core
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using SassAndCoffee.Core.Compilers;
    using SassAndCoffee.Core.Extensions;

    public class ContentCompiler
    {
        private readonly ICompilerHost _host;

        private ICompiledCache _cache;

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

        public bool CanCompile(string inputFileName)
        {
            return _compilers.Any(x => inputFileName.EndsWith(x.OutputFileExtension) && x.FindInputFileGivenOutput(inputFileName) != null);
        }

        public CompilationResult GetCompiledContent (string inputFileName)
        {
            return this._cache.GetOrAdd(inputFileName, this.CompileContent);
        }

        private CompilationResult CompileContent(string inputFileName)
        {
            var physicalFileName = this._host.MapPath(inputFileName);

            if (!File.Exists(physicalFileName))
            {
                return new CompilationResult(false, string.Empty, string.Empty);
            }

            var compiler = this.GetMatchingCompiler(inputFileName);

            if (compiler == null)
            {
                return this.GetFileContents(physicalFileName);
            }

            return new CompilationResult(true, compiler.ProcessFileContent(physicalFileName), compiler.OutputMimeType);
        }

        private CompilationResult GetFileContents(string physicalFileName)
        {
            return new CompilationResult(false, File.ReadAllText(physicalFileName), string.Empty); 
        }

        private void Init()
        {
            foreach (var simpleFileCompiler in _compilers)
            {
                simpleFileCompiler.Init(this._host);
            }
        }

        private ISimpleFileCompiler GetMatchingCompiler(string inputFileName)
        {
            return _compilers.FirstOrDefault(x => inputFileName.EndsWith(x.OutputFileExtension) && x.FindInputFileGivenOutput(inputFileName) != null);
        }
    }
}