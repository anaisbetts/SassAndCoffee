namespace SassAndCoffee.Core
{
    using SassAndCoffee.Core.Caching;

    public interface ICompilerHost
    {
        ICompiledCache Cache { get; }
        IContentCompiler Compiler { get; }
    }
}