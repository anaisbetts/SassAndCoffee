namespace SassAndCoffee.Core.Sass {
    using System;
    using System.Collections.Generic;

    public interface ISassCompiler : IDisposable {
        string Compile(string path, IList<string> dependentFileList = null);
    }
}
