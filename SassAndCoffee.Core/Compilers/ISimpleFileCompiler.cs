namespace SassAndCoffee.Core.Compilers
{
    public interface ISimpleFileCompiler
    {
        string[] InputFileExtensions { get; }
        string OutputFileExtension { get; }
        string OutputMimeType { get; }

        void Init(ICompilerHost host);
        string ProcessFileContent(string inputFileContent);
        string GetFileChangeToken(string inputFileContent);
    }
}