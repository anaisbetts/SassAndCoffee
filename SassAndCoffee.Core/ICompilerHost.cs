namespace SassAndCoffee.Core
{
    public interface ICompilerHost
    {
        ISimpleFileCompiler MapPathToCompiler(string physicalPath);
    }
}