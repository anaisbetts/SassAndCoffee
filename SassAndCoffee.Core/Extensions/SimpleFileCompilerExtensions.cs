using System;
using System.Collections.Generic;

namespace SassAndCoffee.Core.Extensions
{
    using System.IO;

    using SassAndCoffee.Core.Compilers;

    public static class SimpleFileCompilerExtensions
    {
        public static ICompilerFile FindInputFileGivenOutput(this ISimpleFileCompiler This, ICompilerFile outputFilePath) {
            var outputName = outputFilePath.Name;
            if (outputName.EndsWith(This.OutputFileExtension, StringComparison.InvariantCultureIgnoreCase)) {
                outputName = outputName.Substring(0, outputName.Length-This.OutputFileExtension.Length);
                foreach (var ext in This.InputFileExtensions) {
                    ICompilerFile result = outputFilePath.GetRelativeFile(outputName+ext);
                    if (result.Exists) {
                        return result;
                    }
                }
            }
            return null;
        }

        public static TextReader OpenReader(this ICompilerFile This) {
            return new StreamReader(This.Open());
        }

        public static string ReadAllText(this ICompilerFile This) {
            using (var reader = This.OpenReader()) {
                return reader.ReadToEnd();
            }
        }

        public static IEnumerable<string> ReadLines(this ICompilerFile This) {
            using (var reader = This.OpenReader()) {
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine()) {
                    yield return line;
                }
            }
        }
    }
}
