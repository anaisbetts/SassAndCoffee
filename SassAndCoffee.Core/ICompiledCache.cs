namespace SassAndCoffee.Core
{
    using System;

    public interface ICompiledCache
    {
        CompilationResult GetOrAdd(string filename, Func<string, CompilationResult> compilationDelegate);
    }
}