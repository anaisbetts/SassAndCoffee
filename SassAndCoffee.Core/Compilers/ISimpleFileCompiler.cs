namespace SassAndCoffee.Core.Compilers
{
    public interface ISimpleFileCompiler
    {
        string[] InputFileExtensions { get; }
        string OutputFileExtension { get; }
        string OutputMimeType { get; }

        void Init();
        string ProcessFileContent(string inputFileContent);
        string GetFileChangeToken(string inputFileContent);
    }
}