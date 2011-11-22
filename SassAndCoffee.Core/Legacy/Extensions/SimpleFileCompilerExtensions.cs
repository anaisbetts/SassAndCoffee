namespace SassAndCoffee.Core.Extensions
{
    using System.IO;

    using SassAndCoffee.Core.Compilers;

    public static class SimpleFileCompilerExtensions
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
