namespace SassAndCoffee.AspNet
{
    using System.IO;
    using System.Web;

    using SassAndCoffee.Core;
    using SassAndCoffee.Core.Caching;

//#define MEMORY_CACHE

    public class CompilableFileModule : IHttpModule, ICompilerHost
    {
        private IContentCompiler _compiler;

        private IHttpHandler _handler;

        private HttpApplication _application;

        public void Init(HttpApplication context)
        {
            this._application = context;
            // TODO - add web.config entry to allow cache configuration

#if MEMORY_CACHE
            this._compiler = new ContentCompiler(this, new InMemoryCache());
#else
            var cachePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data"), ".sassandcoffeecache");
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }
            this._compiler = new ContentCompiler(this, new FileCache(cachePath)); 
#endif            
            this._handler = new CompilableFileHandler(this._compiler);

            context.PostResolveRequestCache += (o, e) => {
                var app = o as HttpApplication;

                if (!this._compiler.CanCompile(app.Request.Path))
                {
                    return;
                }

                app.Context.RemapHandler(this._handler);
            };
        }

        public string ApplicationBasePath
        {
            get
            {
                return this._application.Request.PhysicalApplicationPath;
            }
        }

        public string MapPath(string path)
        {
            return this._application.Server.MapPath(path);
        }

        public void Dispose()
        {
        }
    }
}
