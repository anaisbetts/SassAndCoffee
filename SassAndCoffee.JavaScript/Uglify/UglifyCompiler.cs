namespace SassAndCoffee.JavaScript.Uglify {
    using System.Collections.Generic;
    using SassAndCoffee.Core;

    public class UglifyCompiler : JavaScriptCompilerBase {
        private readonly static string[] _libs = new string[] {
            "lib.underscore.js",
            "lib.require.js",
            "lib.parse-js.js",
            "lib.process.js",
            "lib.squeeze-more.js",
            "lib.uglify-js.js",
            "lib.compile.js",
        };

        public override IEnumerable<string> CompilerLibraryResourceNames {
            get { return _libs; }
        }

        public override string CompilationFunctionName {
            get { return "compilify_ujs"; }
        }

        public UglifyCompiler(IInstanceProvider<IJavaScriptRuntime> jsRuntimeProvider)
            : base(jsRuntimeProvider) { }
    }
}