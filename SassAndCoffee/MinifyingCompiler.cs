using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using Jurassic;

namespace SassAndCoffee
{
    public class MinifyingCompiler
    {
        static ThreadLocal<ScriptEngine> _engine;
        static ScriptSource _uglifyJsCode;

        static MinifyingCompiler()
        {
            _engine = new ThreadLocal<ScriptEngine>(initializeMinificationEngine);
        }

        public string Compile(string javascriptCode)
        {
            return _engine.Value.CallGlobalFunction<string>("squeeze_it", javascriptCode);
        }

        static ScriptEngine initializeMinificationEngine()
        {
            if (_uglifyJsCode == null) {
                _uglifyJsCode = new StringScriptSource(Utility.ResourceAsString("SassAndCoffee.lib.uglify.js"));
            }

            ScriptEngine se = null;
            var t = new Thread(() => {
                se = new ScriptEngine();
                se.Execute(_uglifyJsCode);
            }, 10 * 1048576);

            t.Start();
            t.Join();

            return se;
        }
    }

    public class MinifyingFileCompiler : ISimpleFileCompiler
    {
        CoffeeScriptCompiler _coffeeEngine;
        MinifyingCompiler _engine;

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
            _coffeeEngine = coffeeScriptEngine;
        }

        public void Init(HttpApplication context)
        {
            _engine = new MinifyingCompiler();
        }

        public string ProcessFileContent(string inputFileContent)
        {
            string text = File.ReadAllText(inputFileContent);

            if (inputFileContent.ToLowerInvariant().EndsWith(".coffee")) {
                text = _coffeeEngine.Compile(text);
            }

            var ret = _engine.Compile(text);
            return ret;
        }
    }
}