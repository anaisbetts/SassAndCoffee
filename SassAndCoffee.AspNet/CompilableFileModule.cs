namespace SassAndCoffee.AspNet
{
    using System.IO;
    using System.Web;

    using SassAndCoffee.Core;
    using SassAndCoffee.Core.Caching;
    using System.Configuration;

//#define MEMORY_CACHE

    public class CompilableFileModule : IHttpModule, ICompilerHost
    {
        IContentCompiler _compiler;

        IHttpHandler _handler;

        public void Init(HttpApplication context)
        {
            var cacheType = ConfigurationManager.AppSettings["SassAndCoffee.Cache"];

            // NoCache
            if (string.Equals(cacheType, "NoCache", System.StringComparison.InvariantCultureIgnoreCase))
            {
                _compiler = new ContentCompiler(this, new NoCache());
            }
            // InMemoryCache
            else if (string.Equals(cacheType, "InMemoryCache", System.StringComparison.InvariantCultureIgnoreCase))
            {
                _compiler = new ContentCompiler(this, new InMemoryCache());
            }
            // FileCache
            else
            {
                var cachePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data"), ".sassandcoffeecache");
                if (!Directory.Exists(cachePath))
                {
                    Directory.CreateDirectory(cachePath);
                }

                _compiler = new ContentCompiler(this, new FileCache(cachePath));
            }

            _handler = new CompilableFileHandler(this._compiler);

            context.PostResolveRequestCache += (o, e) => {
                var app = o as HttpApplication;

                if (!_compiler.CanCompile(app.Request.Path)) {
                    return;
                }

                app.Context.RemapHandler(_handler);
            };
        }

        public string ApplicationBasePath {
            get {
                return HttpContext.Current.Request.PhysicalApplicationPath;
            }
        }

        public string MapPath(string path)
        {
            return HttpContext.Current.Server.MapPath(path);
        }

        public void Dispose()
        {
        }
    }
}
