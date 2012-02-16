namespace SassAndCoffee.JavaScript.CoffeeScript {
    using System.Collections.Generic;
    using SassAndCoffee.Core;

    public class CoffeeScriptCompiler : JavaScriptCompilerBase {
        private readonly static string[] Libs = new[] {
            "lib.coffee-script.js",
            "lib.compile.js",
        };

        public override IEnumerable<string> CompilerLibraryResourceNames {
            get { return Libs; }
        }

        public override string CompilationFunctionName {
            get { return "compilify_cs"; }
        }

        public CoffeeScriptCompiler(IInstanceProvider<IJavaScriptRuntime> jsRuntimeProvider)
            : base(jsRuntimeProvider) { }
    }
}
