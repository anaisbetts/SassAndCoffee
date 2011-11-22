namespace SassAndCoffee.Core.CoffeeScript {
    using SassAndCoffee.Core.Pooling;

    public class CoffeeScriptCompiler : JavaScriptCompilerBase {
        public override string CompilerLibraryResourceName {
            get { return "coffee-script.js"; }
        }

        public override string CompilationFunctionName {
            get { return "compilify_cs"; }
        }

        public CoffeeScriptCompiler(IInstanceProvider<IJavaScriptRuntime> jsRuntimeProvider)
            : base(jsRuntimeProvider) { }
    }
}
