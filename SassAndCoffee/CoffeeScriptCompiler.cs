using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Jurassic;

namespace SassAndCoffee
{
    public class CoffeeScriptCompiler
    {
        static ThreadLocal<ScriptEngine> _engine;
        static ScriptSource _coffeeScriptCode;

        static CoffeeScriptCompiler()
        {
            _engine = new ThreadLocal<ScriptEngine>(initializeCoffeeScriptEngine);
        }

        public string Compile(string coffeeScriptCode)
        {
            return _engine.Value.CallGlobalFunction<string>("compilify", coffeeScriptCode);
        }

        static ScriptEngine initializeCoffeeScriptEngine()
        {
            if (_coffeeScriptCode == null) {
                _coffeeScriptCode = new StringScriptSource(Utility.ResourceAsString("SassAndCoffee.lib.coffee-script.js"));
            }

            ScriptEngine se = null;
            var t = new Thread(() => {
                se = new ScriptEngine();
                se.Execute(_coffeeScriptCode);
            }, 10 * 1048576);

            t.Start();
            t.Join();

            const string compilifyFunction =
                @"function compilify(code) { return CoffeeScript.compile(code, {bare: true}); }";

            se.Execute(compilifyFunction);
            return se;
        }
    }
}
