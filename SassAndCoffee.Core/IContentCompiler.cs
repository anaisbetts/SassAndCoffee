namespace SassAndCoffee.Core
{
    public interface IContentCompiler
    {
        bool CanCompile(string filePath);

        CompilationResult GetCompiledContent(string filePath);

        string GetSourceFileNameFromRequestedFileName(string filePath);
    }
}