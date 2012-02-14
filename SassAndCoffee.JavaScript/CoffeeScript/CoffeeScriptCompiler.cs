namespace SassAndCoffee.JavaScript.CoffeeScript {
    using System.Collections.Generic;
    using SassAndCoffee.Core;

    public class CoffeeScriptCompiler : JavaScriptCompilerBase {
        private readonly static string[] _libs = new string[] {
            "lib.coffee-script.js",
            "lib.compile.js",
        };

        public override IEnumerable<string> CompilerLibraryResourceNames {
            get { return _libs; }
        }

        public override string CompilationFunctionName {
            get { return "compilify_cs"; }
        }

        public CoffeeScriptCompiler(IInstanceProvider<IJavaScriptRuntime> jsRuntimeProvider)
            : base(jsRuntimeProvider) { }
    }
}
