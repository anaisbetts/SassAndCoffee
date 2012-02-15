namespace SassAndCoffee.Core {
    using System;
    using System.Diagnostics.CodeAnalysis;

    [Serializable]
    public class ContentResult {
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Simplify serialization.")]
        public string Content;

        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Simplify serialization.")]
        public string MimeType;

        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Simplify serialization.")]
        public string[] CacheInvalidationFileList;
    }
}
