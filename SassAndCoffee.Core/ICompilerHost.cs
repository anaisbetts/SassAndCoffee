namespace SassAndCoffee.Core
{
    using SassAndCoffee.Core.Compilers;

    public interface ICompilerHost
    {
        ISimpleFileCompiler MapPathToCompiler(string physicalPath);
    }
}