namespace SassAndCoffee.Core {
    using System;
    using System.Collections.Generic;

    public interface ITransformHandler {
        TransformResult HandleRequest(string absolutePath, Dictionary<string, string> parameters);
    }

    public class TransformResult {
        public byte[] Result { get; set; }
        public string MimeType { get; set; }
        public IEnumerable<string> CacheInvalidationFileList { get; set; }
        public DateTime? Expires { get; set; }
    }
}
