namespace SassAndCoffee.Core.Compilers
{
    using System.IO;

    // TODO: Document why this exists
    public class CssPassthroughCompiler : ISimpleFileCompiler
    {
        public string[] InputFileExtensions
        {
            get { return new[] { ".css" }; }
        }

        public string OutputFileExtension
        {
            get { return ".css"; }
        }

        public string OutputMimeType
        {
            get { return "text/css"; }
        }

        public void Init(ICompilerHost host)
        {
        }

        public string ProcessFileContent(string inputFileContent)
        {
            return File.ReadAllText(inputFileContent);
        }

        public string GetFileChangeToken(string inputFileContent)
        {
            return "";
        }
    }
}
