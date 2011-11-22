namespace SassAndCoffee.Core.CoffeeScript {
    using System;
    using System.Collections.Generic;
    using SassAndCoffee.Core.Pooling;
    using System.Text;

    public sealed class JavaScriptTransform : ITransformHandler {

        private IInstanceProvider<IJavaScriptCompiler> _compilerProvider;

        public JavaScriptTransform(IInstanceProvider<IJavaScriptCompiler> compilerProvider) {
            _compilerProvider = compilerProvider;
        }

        public TransformResult HandleRequest(string absolutePath, Dictionary<string, string> parameters) {
            var source = "";

            var result = Encoding.UTF8.GetBytes(Compile(source));

            return new TransformResult() {
                Result = result,
                MimeType = "text/javascript; charset=utf-8",
            };
        }

        private string Compile(string source) {
            using (var compiler = _compilerProvider.GetInstance()) {
                return compiler.Compile(source);
            }
        }
    }
}
