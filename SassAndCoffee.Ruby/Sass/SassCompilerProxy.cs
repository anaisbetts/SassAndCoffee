namespace SassAndCoffee.Ruby.Sass {
    using System;
    using System.Collections.Generic;
    using SassAndCoffee.Core;

    public class SassCompilerProxy : ProxyBase<ISassCompiler>, ISassCompiler {
        public SassCompilerProxy() { }

        public SassCompilerProxy(ISassCompiler compiler)
            : base(compiler) { }

        public string Compile(string path, bool compressed, IList<string> dependentFileList) {
            try {
                return WrappedItem.Compile(path, compressed, dependentFileList);
            } catch (SassSyntaxException) {
                throw;
            } catch {
                // If no SassSyntaxError, then assume IronRuby transient failure.
                ReturnToPool = false;
                throw;
            }
        }

        public void Initialize() {
            throw new NotImplementedException("You must initialize the compiler before pooling it.  Initialization frequently fails.");
        }
    }
}
