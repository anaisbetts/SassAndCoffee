namespace SassAndCoffee.Core {
    using System;

    // TODO: Document me
    public interface ICompiledCache {
        object GetOrAdd(string fileName, Func<string, object> compilationDelegate, string mimeType);

        void Clear();
    }
}
