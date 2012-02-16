namespace SassAndCoffee.Ruby.Sass {
    using System.Collections.Generic;

    public interface ISassCompiler {
        void Initialize();
        string Compile(string path, bool compressed, IList<string> dependentFileList);
    }
}
