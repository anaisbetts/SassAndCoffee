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
        ScriptEngine _engine;
        ScriptSource _uglifyJsCode;

        public string Compile(string javascriptCode)
        {
            if (_engine == null) {
                _engine = initializeMinificationEngine();
            }

            return _engine.CallGlobalFunction<string>("squeeze_it", javascriptCode);
        }

        ScriptEngine initializeMinificationEngine()
        {
            if (this._uglifyJsCode == null) {
                this._uglifyJsCode = new StringScriptSource(Utility.ResourceAsString("SassAndCoffee.lib.uglify.js"));
            }

            ScriptEngine se = null;
            var t = new Thread(() => {
                se = new ScriptEngine();
                se.Execute(this._uglifyJsCode);
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
            try {
                string text = File.ReadAllText(inputFileContent);

                if (inputFileContent.ToLowerInvariant().EndsWith(".coffee")) {
                    text = _coffeeEngine.Compile(text);
                }

                var ret = _engine.Compile(text);
                return ret;
            } catch (Exception ex) {
                return ex.Message;
            }
        }
    }
}
