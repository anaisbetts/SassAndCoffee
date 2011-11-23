namespace SassAndCoffee.AspNet {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using SassAndCoffee.Core.Pipeline;

    public abstract class PathBasedHandlerRemapper : IHttpModule {

        private PipelineHandler _handler;
        private IEnumerable<string> _handledExtensions;

        public abstract IEnumerable<string> HandledExtensions { get; }
        public abstract IEnumerable<IContentTransform> Transformations { get; }

        public void Init(HttpApplication context) {
            _handledExtensions = HandledExtensions;
            _handler = new PipelineHandler(new ContentPipeline(Transformations));
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
                if (_handler != null) {
                    _handler.Dispose();
                    _handler = null;
                }
            }
        }
    }
}
