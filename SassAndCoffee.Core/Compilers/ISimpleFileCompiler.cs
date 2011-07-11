namespace SassAndCoffee.Core.Compilers
{
    // TODO: Document me
    public interface ISimpleFileCompiler
    {
        string[] InputFileExtensions { get; }
        string OutputFileExtension { get; }

        void Init(ICompilerHost host);
        string ProcessFileContent(string inputFileContent);
        string GetFileChangeToken(string inputFileContent);
    }
}
