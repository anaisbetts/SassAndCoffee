namespace SassAndCoffee.AspNet {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;

    public abstract class PathBasedHandlerRemapper<T> : IHttpModule
        where T : class, IHttpHandler, IDisposable, new() {

        private T _handler = new T();

        public abstract IEnumerable<string> HandledExtensions { get; }

        public void Init(HttpApplication context) {
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
