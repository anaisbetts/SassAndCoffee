namespace SassAndCoffee.Ruby.Sass {
    using System;
    using System.Collections.Generic;

    public interface ISassCompiler : IDisposable {
        void Initialize();
        string Compile(string path, bool compressed, IList<string> dependentFileList);
    }
}
