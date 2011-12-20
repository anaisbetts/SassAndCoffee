namespace SassAndCoffee.Ruby.Sass {
    using System.Collections.Generic;
    using SassAndCoffee.Core;

    public class SassCompilerProxy : ProxyBase<ISassCompiler>, ISassCompiler {

        public SassCompilerProxy() { }

        public SassCompilerProxy(ISassCompiler compiler)
            : base(compiler) { }

        public string Compile(string path, bool compressed, IList<string> dependentFileList) {
            return WrappedItem.Compile(path, compressed, dependentFileList);
        }
    }
}
