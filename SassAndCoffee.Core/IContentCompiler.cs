namespace SassAndCoffee.Core
{
    public interface IContentCompiler
    {
        bool CanCompile(string requestedFileName);

        CompilationResult GetCompiledContent (string requestedFileName);

        string GetOutputMimeType(string requestedFileName);
    }
}