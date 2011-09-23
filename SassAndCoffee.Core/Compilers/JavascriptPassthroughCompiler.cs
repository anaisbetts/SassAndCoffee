using System.Collections.Generic;

using SassAndCoffee.Core.Extensions;

namespace SassAndCoffee.Core.Compilers
{
    using System.IO;

    // TODO: Document why this exists
    public class JavascriptPassthroughCompiler : ISimpleFileCompiler
    {
        public IEnumerable<string> InputFileExtensions {
            get {
            	yield return ".js";
            }
        }

        public string OutputFileExtension {
            get { return ".js"; }
        }

        public string OutputMimeType {
            get { return "text/javascript"; }
        }

        public string ProcessFileContent(ICompilerFile inputFileContent)
        {
            return inputFileContent.ReadAllText();
        }

        public string GetFileChangeToken(ICompilerFile inputFileContent) 
        {
            return "";
        }
    }
}
