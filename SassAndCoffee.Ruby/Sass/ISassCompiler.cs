﻿namespace SassAndCoffee.Ruby.Sass {
    using System;
    using System.Collections.Generic;

    public interface ISassCompiler : IDisposable {
        string Compile(string path, bool compressed, IList<string> dependentFileList);
        string CompileScss(string input, bool compressed);
        string CompileSass(string input, bool compressed);
    }
}
