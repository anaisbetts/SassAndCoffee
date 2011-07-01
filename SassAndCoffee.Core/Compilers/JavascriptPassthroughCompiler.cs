namespace SassAndCoffee.Core.Compilers
{
    using System.IO;

    public class JavascriptPassthroughCompiler : ISimpleFileCompiler
    {
        public string[] InputFileExtensions {
            get { return new[] {".js"}; }
        }

        public string OutputFileExtension {
            get { return ".js"; }
        }

        public string OutputMimeType {
            get { return "text/javascript"; }
        }

        public void Init()
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