namespace SassAndCoffee.AspNet {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using SassAndCoffee.Core.Pipeline;

    public abstract class PathBasedHandlerRemapper : IHttpModule, IDisposable {

        private IContentPipeline _pipeline;
        private PipelineHandler _handler;
        private IEnumerable<string> _handledExtensions;

        public abstract IEnumerable<string> HandledExtensions { get; }
        public abstract IEnumerable<IContentTransform> Transformations { get; }

        public void Init(HttpApplication context) {
            _handledExtensions = HandledExtensions;
            _pipeline = new ContentPipeline(Transformations);
            _handler = new PipelineHandler(_pipeline);
            context.PostResolveRequestCache += ConditionallyRemapHandler;
        }

        void ConditionallyRemapHandler(object sender, EventArgs e) {
            var app = sender as HttpApplication;
            var path = app.Request.Path;
            if (HandledExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))) {
                // If the file exists on disk, then we don't need to transform it.
                if (!File.Exists(app.Request.PhysicalPath)) {
                    app.Context.RemapHandler(_handler);
                }
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_pipeline != null) {
                    _pipeline.Dispose();
                    _pipeline = null;
                }
            }
        }
    }
}
