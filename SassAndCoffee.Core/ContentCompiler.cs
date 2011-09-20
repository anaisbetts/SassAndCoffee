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
        private readonly ICompiledCache _cache;

        private readonly IEnumerable<ISimpleFileCompiler> _compilers;

        public ContentCompiler(ICompiledCache cache)
        {
            _cache = cache;

            _compilers = new ISimpleFileCompiler[] {
                new FileConcatenationCompiler(this),
                new MinifyingFileCompiler(),
                new CoffeeScriptFileCompiler(),
                new SassFileCompiler(),
                new JavascriptPassthroughCompiler(),
            };
        }

        public ContentCompiler(ICompiledCache cache, IEnumerable<ISimpleFileCompiler> compilers)
        {
            _cache = cache;
            _compilers = compilers;
        }

        public bool CanCompile(ICompilerFile physicalFileName)
        {
            return GetMatchingCompiler(physicalFileName) != null;
        }

		public CompilationResult GetCompiledContent(ICompilerFile sourceFileName)
        {
            var compiler = GetMatchingCompiler(sourceFileName);
            if (compiler == null) {
                return CompilationResult.Error;
            }

            var physicalFileName = compiler.FindInputFileGivenOutput(sourceFileName);
            if (!physicalFileName.Exists) {
                return CompilationResult.Error;
            }

            var cacheKey = GetCacheKey(physicalFileName, compiler);
            return _cache.GetOrAdd(cacheKey, f => CompileContent(physicalFileName, compiler), compiler.OutputMimeType);
        }

		public ICompilerFile GetSourceFileNameFromRequestedFileName(ICompilerFile physicalFileName)
        {
            var compiler = GetMatchingCompiler(physicalFileName);
            if (compiler == null) {
                return null;
            }

            return compiler.FindInputFileGivenOutput(physicalFileName);
        }

        public string GetOutputMimeType(ICompilerFile requestedFileName)
        {
            var compiler = GetMatchingCompiler(requestedFileName);
            if (compiler == null) {
                return "application/octet-stream";
            }

            return compiler.OutputMimeType;
        }

        private string GetCacheKey(ICompilerFile physicalFileName, ISimpleFileCompiler compiler)
        {
            var token = compiler.GetFileChangeToken(physicalFileName) ?? String.Empty;

            return String.Format("{0:yyyyMMddHHmmss}-{1}-{2}{3}",
                physicalFileName.LastWriteTimeUtc, token,
                Path.GetFileNameWithoutExtension(physicalFileName.Name),
                compiler.OutputFileExtension);
        }

        private CompilationResult CompileContent(ICompilerFile physicalFileName, ISimpleFileCompiler compiler)
        {
            return new CompilationResult(true, compiler.ProcessFileContent(physicalFileName), compiler.OutputMimeType, physicalFileName.LastWriteTimeUtc);
        }

        private ISimpleFileCompiler GetMatchingCompiler(ICompilerFile physicalPath)
        {
            return _compilers.FirstOrDefault(x => physicalPath.Name.EndsWith(x.OutputFileExtension, StringComparison.OrdinalIgnoreCase) && x.FindInputFileGivenOutput(physicalPath) != null);
        }
    }
}