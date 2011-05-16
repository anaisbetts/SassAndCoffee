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
        ScriptEngine _engine;
        ScriptSource _coffeeScriptCode;

        public string Compile(string coffeeScriptCode)
        {
            if (_engine == null)
            {
                _engine = initializeCoffeeScriptEngine();
            }

            return _engine.CallGlobalFunction<string>("compilify", coffeeScriptCode);
        }

        ScriptEngine initializeCoffeeScriptEngine()
        {
            if (_coffeeScriptCode == null)
            {
                var ms = new MemoryStream();
                Assembly.GetExecutingAssembly().GetManifestResourceStream("SassAndCoffee.lib.coffee-script.js").CopyTo(ms);

                var str = Encoding.ASCII.GetString(ms.GetBuffer());
                _coffeeScriptCode = new StringScriptSource(str.Replace('\0', ' ').Trim());
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
