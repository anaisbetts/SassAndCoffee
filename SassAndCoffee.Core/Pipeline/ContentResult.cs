namespace SassAndCoffee.Core {
    using System;

    [Serializable]
    public class ContentResult {
        public string Content { get; set; }
        public string MimeType { get; set; }
        public string[] CacheInvalidationFileList { get; set; }
    }
}
