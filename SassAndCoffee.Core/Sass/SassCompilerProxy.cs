namespace SassAndCoffee.Core.Sass {
    using System.Collections.Generic;
    using SassAndCoffee.Core.Pooling;

    public class SassCompilerProxy : ProxyBase<ISassCompiler>, ISassCompiler {

        public SassCompilerProxy() { }

        public SassCompilerProxy(ISassCompiler compiler)
            : base(compiler) { }

        public string Compile(string path, IList<string> dependentFileList = null) {
            return WrappedItem.Compile(path, dependentFileList);
        }
    }
}
