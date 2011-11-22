namespace SassAndCoffee.Core.Uglify {
    using SassAndCoffee.Core.Pooling;

    public class UglifyCompiler : JavaScriptCompilerBase {
        public override string CompilerLibraryResourceName {
            get { return "uglify.js"; }
        }

        public override string CompilationFunctionName {
            get { return "compilify_ujs"; }
        }

        public UglifyCompiler(IInstanceProvider<IJavaScriptRuntime> jsRuntimeProvider)
            : base(jsRuntimeProvider) { }
    }
}