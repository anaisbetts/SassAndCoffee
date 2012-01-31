namespace SassAndCoffee.AspNet {
    using System;
    using System.Configuration;
    using System.IO;
    using System.Web;
    using SassAndCoffee.Core;

    /// <summary>
    /// An HttpModule that will conditionally handle requests for files with certain extensions.
    /// </summary>
    public class PathBasedHandlerRemapper : IHttpModule, IDisposable {
        public const string SassAndCoffeeCacheTypeKey = "SassAndCoffee.Cache";
        public const string SassAndCoffeeCachePathKey = "SassAndCoffee.Cache.Path";
        public const string AppDataSpecialKey = "%DataDirectory%";

        private IContentCache _cache;
        private IContentPipeline _pipeline;
        private PipelineHandler _handler;
        private string _handledExtension;
        private IContentTransform[] _transformations;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathBasedHandlerRemapper"/> class.
        /// </summary>
        /// <param name="handledExtension">The handled extension.  Include the '.': ".css"</param>
        /// <param name="transformations">The transformations to apply in the content pipeline.</param>
        public PathBasedHandlerRemapper(string handledExtension, params IContentTransform[] transformations) {
            _handledExtension = handledExtension;
            _transformations = transformations;
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to
        /// the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context) {
            // This feels dirty, and isn't extensible.  Web.config module registrations are so limiting!
            // TODO: This technically makes two caches if the user enables both CoffeeScript and Sass/SCSS
            _cache = GetCacheFromSettings();
            _cache.Initialize();
            _pipeline = new CachingContentPipeline(_cache, _transformations);
            _handler = new PipelineHandler(_pipeline);

            context.PostResolveRequestCache += ConditionallyRemapHandler;
        }

        /// <summary>
        /// Conditionally remaps the handler for the current request if the requested file
        /// * Does not exist
        /// * Matches the handled extension
        /// </summary>
        /// <param name="sender">The HttpApplication.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void ConditionallyRemapHandler(object sender, EventArgs e) {
            var app = sender as HttpApplication;
            var path = app.Request.Path;
            if (path.EndsWith(_handledExtension, StringComparison.OrdinalIgnoreCase)) {
                // If the file exists on disk, then we don't need to transform it.
                if (!File.Exists(app.Request.PhysicalPath)) {
                    app.Context.RemapHandler(_handler);
                }
            }
        }

        /// <summary>
        /// Gets the correct IContentCache implementation from the application settings.
        /// </summary>
        protected virtual IContentCache GetCacheFromSettings() {
            var cacheSetting = ConfigurationManager.AppSettings[SassAndCoffeeCacheTypeKey];
            if (cacheSetting == "Memory") {
                return new InMemoryCache();
            } else if (cacheSetting == "File") {
                var path = ConfigurationManager.AppSettings[SassAndCoffeeCachePathKey];
                if (path.StartsWith(AppDataSpecialKey)) {
                    path = path.Substring(AppDataSpecialKey.Length);
                    path = path.TrimStart(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                    path = Path.Combine(
                        AppDomain.CurrentDomain.GetData("DataDirectory").ToString(),
                        path);
                }
                return new FileCache(path);
            } else return new NoCache();
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c>
        /// to release only unmanaged resources.</param>
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
