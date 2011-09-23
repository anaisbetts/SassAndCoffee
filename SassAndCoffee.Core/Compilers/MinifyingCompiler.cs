using System;
using System.Collections.Generic;

using SassAndCoffee.Core.Extensions;

namespace SassAndCoffee.Core.Compilers
{
    using System.IO;

    public class MinifyingCompiler : JavascriptBasedCompiler
    {
        public MinifyingCompiler() : base("SassAndCoffee.Core.lib.uglify.js", "compilify_ujs") { }
    }

    public class MinifyingFileCompiler : ISimpleFileCompiler
    {
        TrashStack<CoffeeScriptCompiler> _coffeeEngine;
        TrashStack<MinifyingCompiler> _engine;

        public IEnumerable<string> InputFileExtensions {
            get {
                yield return ".js";
                yield return ".coffee";
            }
        }

        public string OutputFileExtension {
            get { return ".min.js"; }
        }

        public string OutputMimeType {
            get { return "text/javascript"; }
        }

        public MinifyingFileCompiler()
        {
            _coffeeEngine = new TrashStack<CoffeeScriptCompiler>(() => new CoffeeScriptCompiler());
            _engine = new TrashStack<MinifyingCompiler>(() => new MinifyingCompiler());
        }

		public string ProcessFileContent(ICompilerFile inputFileContent)
        {
            string text = inputFileContent.ReadAllText();
		    if (inputFileContent.Name.EndsWith(".coffee", StringComparison.OrdinalIgnoreCase)) {
		        using (ValueContainer<CoffeeScriptCompiler> coffeeEngine = _coffeeEngine.Get()) {
		            text = coffeeEngine.Value.Compile(text);
		        }
		    }
		    string ret;
		    using (ValueContainer<MinifyingCompiler> engine = _engine.Get()) {
		        ret = engine.Value.Compile(text);
		    }
		    return ret;
        }

		public string GetFileChangeToken(ICompilerFile inputFileContent)
        {
            return "";
        }
    }
}