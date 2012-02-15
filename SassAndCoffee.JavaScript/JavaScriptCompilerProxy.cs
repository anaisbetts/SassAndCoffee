namespace SassAndCoffee.JavaScript {
    using System.Diagnostics.CodeAnalysis;
    using SassAndCoffee.Core;

    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It has to be this way and it works fine.")]
    public class JavaScriptCompilerProxy : ProxyBase<IJavaScriptCompiler>, IJavaScriptCompiler {

        public JavaScriptCompilerProxy() { }

        public JavaScriptCompilerProxy(IJavaScriptCompiler compiler)
            : base(compiler) { }

        public string Compile(string source, params object[] args) {
            return WrappedItem.Compile(source, args);
        }
    }
}

