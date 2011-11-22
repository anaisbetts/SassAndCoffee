namespace SassAndCoffee.Core.Sass {
    using SassAndCoffee.Core.Pooling;

    public class SassCompilerProxy : ProxyBase<ISassCompiler>, ISassCompiler {

        public SassCompilerProxy() { }

        public SassCompilerProxy(ISassCompiler compiler)
            : base(compiler) { }

        public string Compile(string path) {
            return WrappedItem.Compile(path);
        }
    }
}
