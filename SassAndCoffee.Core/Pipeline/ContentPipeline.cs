namespace SassAndCoffee.Core {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ContentPipeline : IContentPipeline {
        private List<IContentTransform> _transformations = new List<IContentTransform>();

        public IList<IContentTransform> Transformations { get { return _transformations; } }

        public ContentPipeline(params IContentTransform[] transformations)
            : this((IEnumerable<IContentTransform>)transformations) { }

        public ContentPipeline(IEnumerable<IContentTransform> transformations) {
            _transformations.AddRange(transformations);
        }

        public ContentResult ProcessRequest(string physicalPath) {
            var state = new ContentTransformState(this, physicalPath);

            // Pre-Execute
            foreach (var transform in _transformations) {
                transform.PreExecute(state);
            }

            // Execute
            foreach (var transform in _transformations) {
                transform.Execute(state);
            }

            if (state.Content == null) {
                // No source content found
                return null;
            }

            return new ContentResult() {
                CacheInvalidationFileList = state.CacheInvalidationFileList.ToArray(),
                Content = state.Content,
                MimeType = state.MimeType,
            };
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_transformations != null) {
                    foreach (var item in _transformations)
                        item.Dispose();
                    _transformations = null;
                }
            }
        }
    }
}
