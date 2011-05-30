using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace SassAndCoffee
{
    public interface ICompilerHost
    {
        ISimpleFileCompiler MapPathToCompiler(string physicalPath);
    }

    public interface ISimpleFileCompiler
    {
        string[] InputFileExtensions { get; }
        string OutputFileExtension { get; }
        string OutputMimeType { get; }

        void Init(HttpApplication context);
        string ProcessFileContent(string inputFileContent);
    }

    public static class FileCompilerMixins
    {
        public static string FindInputFileGivenOutput(this ISimpleFileCompiler This, string outputFilePath)
        {
            var rootFi = new FileInfo(outputFilePath);

            foreach (var ext in This.InputFileExtensions) {
                var fi = new FileInfo(Path.Combine(rootFi.DirectoryName,
                    rootFi.FullName.ToLowerInvariant().Replace(This.OutputFileExtension, "") + ext));

                if (fi.Exists) {
                    return fi.FullName;
                }
            }

            return null;
        }
    }
}
