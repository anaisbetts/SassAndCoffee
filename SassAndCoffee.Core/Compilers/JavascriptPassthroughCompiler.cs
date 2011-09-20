using System.Collections.Generic;

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
            using (TextReader reader = inputFileContent.Open()) {
                return reader.ReadToEnd();
            }
        }

        public string GetFileChangeToken(ICompilerFile inputFileContent) 
        {
            return "";
        }
    }
}
