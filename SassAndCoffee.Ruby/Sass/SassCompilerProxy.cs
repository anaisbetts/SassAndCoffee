namespace SassAndCoffee.Ruby.Sass {
    using System.Collections.Generic;
    using SassAndCoffee.Core;
    using System;

    public class SassCompilerProxy : ProxyBase<ISassCompiler>, ISassCompiler {

        public SassCompilerProxy() { }

        public SassCompilerProxy(ISassCompiler compiler)
            : base(compiler) { }

        public string Compile(string path, bool compressed, IList<string> dependentFileList) {
            return WrappedItem.Compile(path, compressed, dependentFileList);
        }

        public void Initialize() {
            throw new NotImplementedException("You must initialize the compiler before pooling it.  Initialization frequently fails.");
        }
    }
}
