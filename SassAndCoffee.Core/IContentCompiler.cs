namespace SassAndCoffee.Core
{
    public interface IContentCompiler
    {
        bool CanCompile(ICompilerFile requestedFileName);

        CompilationResult GetCompiledContent(ICompilerFile requestedFileName);

        ICompilerFile GetSourceFileNameFromRequestedFileName(ICompilerFile requestedFileName);

        string GetOutputMimeType(ICompilerFile requestedFileName);
    }
}