namespace SassAndCoffee.Core.Extensions
{
    using System.IO;

    using SassAndCoffee.Core.Compilers;

    public static class SimpleFileCompilerExtensions
    {
        public static ICompilerFile FindInputFileGivenOutput(this ISimpleFileCompiler This, ICompilerFile outputFilePath)
        {
            foreach (var ext in This.InputFileExtensions) {
                ICompilerFile result = outputFilePath.GetRelativeFile(Path.ChangeExtension(outputFilePath.Name, ext));
                if (result.Exists) {
                    return result;
                }
            }
            return null;
        }
    }
}
