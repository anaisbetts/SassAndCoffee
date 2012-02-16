namespace SassAndCoffee.Core {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class ContentTransformState {
        private readonly StringBuilder _content = new StringBuilder();
        private readonly List<string> _cacheInvalidationFileList = new List<string>();
        private readonly Dictionary<string, object> _items = new Dictionary<string, object>();

        public string Content { get { return (_content.Length == 0) ? null : _content.ToString(); } }
        public IDictionary<string, object> Items { get { return _items; } }
        public IEnumerable<string> CacheInvalidationFileList {
            get { return _cacheInvalidationFileList; }
        }

        public IContentPipeline Pipeline { get; set; }
        public string Path { get; private set; }
        public string RootPath { get; private set; }
        public string MimeType { get; private set; }

        public ContentTransformState(IContentPipeline pipeline, string physicalPath) {
            Pipeline = pipeline;
            RemapPath(physicalPath);
        }

        public void RemapPath(string newPhysicalPath) {
            Path = newPhysicalPath;
            RootPath = GetRootPath(Path);
        }

        public void AppendContent(ContentResult append) {
            if (append == null)
                throw new ArgumentNullException("append", "append cannot be null.");
            if (append.Content == null)
                throw new ArgumentNullException("append", "append.Content cannot be null.");

            if (_content.Length != 0
                && append.MimeType != null
                && MimeType != null
                && MimeType != append.MimeType) {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Invalid attempt to combine content with different types: {0} and {1}",
                    MimeType,
                    append.MimeType));
            }

            _content.AppendLine(append.Content);
            MimeType = append.MimeType;
            MergeCacheInvalidationFileList(append.CacheInvalidationFileList);
        }

        public void ReplaceContent(ContentResult replace) {
            if (replace == null)
                throw new ArgumentNullException("replace", "replace cannot be null.");

            _content.Clear();
            _content.AppendLine(replace.Content);
            MimeType = replace.MimeType;
            MergeCacheInvalidationFileList(replace.CacheInvalidationFileList);
        }

        public void AddCacheInvalidationFiles(IEnumerable<string> cacheInvalidationFileList) {
            MergeCacheInvalidationFileList(cacheInvalidationFileList);
        }

        private void MergeCacheInvalidationFileList(IEnumerable<string> cacheInvalidationFileList) {
            if (cacheInvalidationFileList != null) {
                var newFiles = cacheInvalidationFileList
                    .Where(File.Exists) // Skip directories and failures
                    .Except(_cacheInvalidationFileList, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (newFiles.Any()) {
                    _cacheInvalidationFileList.AddRange(newFiles);
                }
            }
        }

        private static string GetRootPath(string physicalPath) {
            var lastDot = physicalPath.LastIndexOf('.');
            if (lastDot < 0)
                return null;

            return physicalPath.Substring(0, lastDot);
        }
    }

}
