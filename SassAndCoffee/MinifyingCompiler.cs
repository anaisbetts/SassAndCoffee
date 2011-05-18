using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
}
