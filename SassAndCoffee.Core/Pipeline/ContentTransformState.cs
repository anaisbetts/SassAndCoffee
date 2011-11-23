namespace SassAndCoffee.Core.Pipeline {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class ContentTransformState {
        public const int MaxContentAgeSeconds = 3600;    // Cache for 1 hour max by default

        private int _maxAgeSeconds = MaxContentAgeSeconds;
        private List<string> _cacheInvalidationFileList = new List<string>();
        private Dictionary<string, object> _items = new Dictionary<string, object>();

        public int MaxAgeSeconds { get { return _maxAgeSeconds; } }
        public IDictionary<string, object> Items { get { return _items; } }
        public IEnumerable<string> CacheInvalidationFileList {
            get { return _cacheInvalidationFileList; }
        }

        public IContentPipeline Pipeline { get; set; }
        public string Path { get; private set; }
        public string RootPath { get; private set; }
        public string Content { get; private set; }
        public string MimeType { get; private set; }

        public ContentTransformState(IContentPipeline pipeline, string physicalPath) {
            Pipeline = pipeline;
            RemapPath(physicalPath);
        }

        public void RemapPath(string newPhysicalPath) {
            Path = newPhysicalPath;
            RootPath = GetRootPath(Path);
        }

        public void AppendContent(ContentResult content) {
            if (content == null)
                throw new ArgumentNullException("content", "content cannot be null.");
            if (content.Content == null)
                throw new ArgumentNullException("content.Content", "content.Content cannot be null.");

            if (Content == null) {
                // First item
                Content = content.Content;
                MimeType = content.MimeType;
            } else {
                if (content.MimeType != null) {
                    if (MimeType != null && MimeType != content.MimeType) {
                        throw new InvalidOperationException(string.Format(
                            "Invalid attempt to combine content with different MimeType {0} and {1}",
                            MimeType,
                            content.MimeType));
                    }
                    MimeType = content.MimeType;
                    Content = content + content.Content;
                }
            }

            MergeCacheInvalidationFileList(content.CacheInvalidationFileList);
            CoalesceMaxAge(content.MaxAgeSeconds);
        }

        public void ReplaceContent(ContentResult content) {
            if (content == null)
                throw new ArgumentNullException("content", "content cannot be null.");

            Content = content.Content;
            MimeType = content.MimeType;

            MergeCacheInvalidationFileList(content.CacheInvalidationFileList);
            CoalesceMaxAge(content.MaxAgeSeconds);
        }

        private void MergeCacheInvalidationFileList(IEnumerable<string> cacheInvalidationFileList) {
            if (cacheInvalidationFileList != null && cacheInvalidationFileList.Any()) {
                var newFiles = cacheInvalidationFileList
                    .Where(f => File.Exists(f)) // Skip directories and failures
                    .Except(_cacheInvalidationFileList, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (newFiles.Any()) {
                    _cacheInvalidationFileList.AddRange(newFiles);
                }
            }
        }

        private void CoalesceMaxAge(int? maxAgeSeconds) {
            if (maxAgeSeconds.HasValue)
                _maxAgeSeconds = Math.Min(_maxAgeSeconds, maxAgeSeconds.Value);
        }

        private string GetRootPath(string physicalPath) {
            var lastDot = physicalPath.LastIndexOf('.');
            if (lastDot < 0)
                return null;

            return physicalPath.Substring(0, lastDot);
        }
    }

}
