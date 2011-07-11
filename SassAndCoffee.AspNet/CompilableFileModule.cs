namespace SassAndCoffee.AspNet
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.IO;
    using System.Web;
    using SassAndCoffee.Core;
    using SassAndCoffee.Core.Caching;

    public class CompilableFileModule : IHttpModule, ICompilerHost
    {
        IContentCompiler _compiler;

        IHttpHandler _handler;

        public void Init(HttpApplication context)
        {
            var cacheType = ConfigurationManager.AppSettings["SassAndCoffee.Cache"];
            var cachePath = ConfigurationManager.AppSettings["SassAndCoffee.CachePath"];
            if (string.IsNullOrWhiteSpace(cachePath))
                cachePath = "~/App_Data/.sassandcoffeecache";

            _compiler = new ContentCompiler(this, CreateCache(cacheType, HttpContext.Current.Server.MapPath(cachePath)));
            _handler = new CompilableFileHandler(_compiler);

            context.PostResolveRequestCache += (o, e) => {
                var app = o as HttpApplication;

                if (_compiler.CanCompile(app.Request.PhysicalPath)) {
                    app.Context.RemapHandler(_handler);
                }
            };
        }

        public string ApplicationBasePath {
            get {
                return HttpContext.Current.Request.PhysicalApplicationPath;
            }
        }

        public void Dispose()
        {
        }

        static ICompiledCache CreateCache(string cacheType, string cachePath)
        {
            if (string.Equals("NoCache", cacheType, StringComparison.InvariantCultureIgnoreCase))
                return new NoCache();

            if (string.Equals("InMemoryCache", cacheType, StringComparison.InvariantCultureIgnoreCase))
                return new InMemoryCache();

            if (!Directory.Exists(cachePath))
                Directory.CreateDirectory(cachePath);

            return new FileCache(cachePath);
        }
    }
}
