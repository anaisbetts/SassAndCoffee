namespace SassAndCoffee.Core {
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Caches results content results using the provided cache implementation.
    /// </summary>
    public class ContentPipeline : IContentPipeline {
        private readonly List<IContentTransform> _transformations = new List<IContentTransform>();
        private readonly IContentCache _cache;

        public IList<IContentTransform> Transformations { get { return _transformations; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentPipeline"/> class without caching.
        /// </summary>
        /// <param name="transformations">The transformations with which to populate the pipeline.</param>
        public ContentPipeline(params IContentTransform[] transformations)
            : this(null, (IEnumerable<IContentTransform>)transformations) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentPipeline"/> class without caching.
        /// </summary>
        /// <param name="transformations">The transformations with which to populate the pipeline.</param>
        public ContentPipeline(IEnumerable<IContentTransform> transformations)
            : this(null, transformations) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentPipeline"/> class.
        /// </summary>
        /// <param name="cache">The cache implementation to use.</param>
        /// <param name="transformations">The transformations with which to populate the pipeline.</param>
        public ContentPipeline(IContentCache cache, params IContentTransform[] transformations)
            : this(cache, (IEnumerable<IContentTransform>)transformations) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentPipeline"/> class.
        /// </summary>
        /// <param name="cache">The cache implementation to use.</param>
        /// <param name="transformations">The transformations with which to populate the pipeline.</param>
        public ContentPipeline(IContentCache cache, IEnumerable<IContentTransform> transformations) {
            _cache = cache ?? NoCache.Instance;
            _transformations.AddRange(transformations);
        }

        public ContentResult ProcessRequest(string physicalPath) {
            // Attempt to normalize out ./.././../ style stuff.
            var resource = new FileInfo(physicalPath).FullName;
            return _cache.GetOrAdd(resource, Execute);
        }

        private ContentResult Execute(string physicalPath) {
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

            return new ContentResult {
                CacheInvalidationFileList = state.CacheInvalidationFileList.ToArray(),
                Content = state.Content,
                MimeType = state.MimeType,
            };
        }
    }
}
