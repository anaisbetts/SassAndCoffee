using System;
using System.Collections.Generic;

using SassAndCoffee.Core.Extensions;

namespace SassAndCoffee.Core.Compilers
{
    // NB: This class seems stupid, but it makes it easier for other projects 
    // to reuse the CoffeeScript compilation bits without committing to the caching
    // logic
    public class CoffeeScriptCompiler : JavascriptBasedCompiler
    {
        public CoffeeScriptCompiler() : base("SassAndCoffee.Core.lib.coffee-script.js", "compilify_cs") { }
    }

    public class CoffeeScriptFileCompiler : ISimpleFileCompiler
    {
        private readonly Lazy<CoffeeScriptCompiler> _engine;

        public IEnumerable<string> InputFileExtensions {
            get { yield return ".coffee"; }
        }

        public string OutputFileExtension {
            get { return ".js"; }
        }

        public string OutputMimeType {
            get { return "text/javascript"; }
        }

        public CoffeeScriptFileCompiler(CoffeeScriptCompiler engine = null)
        {
            _engine = new Lazy<CoffeeScriptCompiler>(() => engine ?? new CoffeeScriptCompiler());
        }

        public string ProcessFileContent(ICompilerFile inputFileContent)
        {
            return _engine.Value.Compile(inputFileContent.ReadAllText());
        }

        public string GetFileChangeToken(ICompilerFile inputFileContent)
        {
            return "";
        }
    }
}
