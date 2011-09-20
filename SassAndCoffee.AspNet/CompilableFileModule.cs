using System.Web.Hosting;

namespace SassAndCoffee.AspNet
{
    using System.IO;
    using System.Web;

    using SassAndCoffee.Core;
    using SassAndCoffee.Core.Caching;
    using System.Configuration;

    public class CompilableFileModule : IHttpModule
    {
        IContentCompiler _compiler;
        IHttpHandler _handler;

        public string ApplicationBasePath {
            get {
                return HttpContext.Current.Request.PhysicalApplicationPath;
            }
        }

        public void Init(HttpApplication context)
        {
            var cacheType = ConfigurationManager.AppSettings["SassAndCoffee.Cache"];

            context.PostResolveRequestCache += (o, e) => {
                var app = o as HttpApplication;
                _compiler = _compiler ?? initializeCompilerFromSettings(cacheType);
                _handler = _handler ?? new CompilableFileHandler(_compiler);

                if (!_compiler.CanCompile(new VirtualPathCompilerFile(app.Request.Path))) {
                    return;
                }

                app.Context.RemapHandler(_handler);
            };
        }

        public string MapPath(string path)
        {
            return HttpContext.Current.Server.MapPath(path);
        }

        IContentCompiler initializeCompilerFromSettings(string cacheType) {
            if (string.Equals(cacheType, "NoCache", System.StringComparison.InvariantCultureIgnoreCase)) {
                // NoCache
                return new ContentCompiler(new NoCache());
            }
            if (string.Equals(cacheType, "InMemoryCache", System.StringComparison.InvariantCultureIgnoreCase)) {
                // InMemoryCache
                return new ContentCompiler(new InMemoryCache());
            }
            // FileCache
            var cachePath = Path.Combine(HostingEnvironment.MapPath("~/App_Data"), "_FileCache");
            Directory.CreateDirectory(cachePath);
            return new ContentCompiler(new FileCache(cachePath));
        }

        public void Dispose()
        {
        }
    }
}
