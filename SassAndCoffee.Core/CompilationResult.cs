namespace SassAndCoffee.Core
{
    public sealed class CompilationResult
    {
        public bool Compiled { get; private set; }

        public string Contents { get; private set; }

        public string MimeType { get; private set; }

        public CompilationResult(bool compiled, string contents, string mimeType)
        {
            this.Compiled = compiled;
            this.Contents = contents;
            this.MimeType = mimeType;
        }
    }
}