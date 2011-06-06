using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using Jurassic;
using V8Bridge.Interface;

namespace SassAndCoffee
{
    public class MinifyingCompiler : JavascriptBasedCompiler
    {
        public MinifyingCompiler() : base("SassAndCoffee.lib.uglify.js", "compilify_ujs") { }
    }

    public class MinifyingFileCompiler : ISimpleFileCompiler
    {
        ThreadLocal<CoffeeScriptCompiler> _coffeeEngine;
        ThreadLocal<MinifyingCompiler> _engine;

        public string[] InputFileExtensions {
            get { return new[] {".js", ".coffee"}; }
        }

        public string OutputFileExtension {
            get { return ".min.js"; }
        }

        public string OutputMimeType {
            get { return "text/javascript"; }
        }

        public MinifyingFileCompiler(CoffeeScriptCompiler coffeeScriptEngine)
        {
            _coffeeEngine = new ThreadLocal<CoffeeScriptCompiler>();
            _engine = new ThreadLocal<MinifyingCompiler>();
        }

        public void Init(HttpApplication context)
        {
        }

        public string ProcessFileContent(string inputFileContent)
        {
            string text = File.ReadAllText(inputFileContent);

            if (_coffeeEngine.Value == null) {
                _coffeeEngine.Value = new CoffeeScriptCompiler();
            }

            if (_engine.Value == null) {
                _engine.Value = new MinifyingCompiler();
            }

            if (inputFileContent.ToLowerInvariant().EndsWith(".coffee")) {
                text = _coffeeEngine.Value.Compile(text);
            }

            var ret = _engine.Value.Compile(text);
            return ret;
        }

        public string GetFileChangeToken(string inputFileContent)
        {
            return "";
        }
    }
}