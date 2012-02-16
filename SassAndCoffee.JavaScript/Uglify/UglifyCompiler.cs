namespace SassAndCoffee.JavaScript.Uglify {
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using SassAndCoffee.Core;

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Uglify")]
    public class UglifyCompiler : JavaScriptCompilerBase {
        private readonly static string[] Libs = new[] {
            "lib.underscore.js",
            "lib.require.js",
            "lib.parse-js.js",
            "lib.process.js",
            "lib.squeeze-more.js",
            "lib.uglify-js.js",
            "lib.compile.js",
        };

        public override IEnumerable<string> CompilerLibraryResourceNames {
            get { return Libs; }
        }

        public override string CompilationFunctionName {
            get { return "compilify_ujs"; }
        }

        public UglifyCompiler(IInstanceProvider<IJavaScriptRuntime> jsRuntimeProvider)
            : base(jsRuntimeProvider) { }
    }
}