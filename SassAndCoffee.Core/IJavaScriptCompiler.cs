namespace SassAndCoffee.Core {
    using System;

    public interface IJavaScriptCompiler : IDisposable {
        string Compile(string source);
    }
}
