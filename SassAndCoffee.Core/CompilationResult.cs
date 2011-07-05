namespace SassAndCoffee.Core
{
    using System;

    public sealed class CompilationResult
    {

        public bool Compiled { get; private set; }

        public string Contents { get; private set; }

        public string MimeType { get; private set; }

        public DateTime SourceLastModifiedUtc { get; private set; }

        private static readonly CompilationResult _error = new CompilationResult(false, String.Empty, String.Empty, DateTime.MinValue);
        public static CompilationResult Error
        {
            get
            {
                return _error;
            }
        }

        public CompilationResult(bool compiled, string contents, string mimeType, DateTime sourceLastUpdated)
        {
            this.Compiled = compiled;
            this.Contents = contents;
            this.MimeType = mimeType;
            this.SourceLastModifiedUtc = sourceLastUpdated;
        }
    }
}