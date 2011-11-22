namespace SassAndCoffee.Core.Sass {
    using System;

    public interface ISassCompiler : IDisposable {
        string Compile(string path);
    }
}
