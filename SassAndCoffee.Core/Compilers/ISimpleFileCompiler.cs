using System.Collections.Generic;
using System.IO;

namespace SassAndCoffee.Core.Compilers
{
    // TODO: Document me
    public interface ISimpleFileCompiler
    {
        IEnumerable<string> InputFileExtensions { get; }
        string OutputFileExtension { get; }
        string OutputMimeType { get; }

        string ProcessFileContent(ICompilerFile inputFileContent);
        string GetFileChangeToken(ICompilerFile inputFileContent);
    }
}
