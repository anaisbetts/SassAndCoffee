namespace SassAndCoffee.AspNet
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Web;
    using SassAndCoffee.Core;
    using SassAndCoffee.Core.Caching;

    public class CompilableFileModule : IHttpModule, ICompilerHost
    {
        ICompiledCache _cache;
        internal IContentCompiler _compiler;
        IHttpHandler _handler;

        public void Init(HttpApplication context)
        {
            var cacheType = ConfigurationManager.AppSettings["SassAndCoffee.Cache"];
            var cachePath = ConfigurationManager.AppSettings["SassAndCoffee.CachePath"];
            if (string.IsNullOrWhiteSpace(cachePath))
                cachePath = "~/App_Data/.sassandcoffeecache";

            this._cache = CreateCache(cacheType, HttpContext.Current.Server.MapPath(cachePath));
            this._compiler = new ContentCompiler(this);
            this._handler = new CompilableFileHandler(this);

            context.PostResolveRequestCache += (o, e) => {
                var app = o as HttpApplication;

                if (this._compiler.CanCompile(app.Request.PhysicalPath))
                {
                    app.Context.RemapHandler(this._handler);
                }
            };
        }

        public ICompiledCache Cache
        {
            get
            {
                return _cache;
            }
        }

        static ICompiledCache CreateCache(string cacheType, string cachePath)
        {
            if (string.Equals("NoCache", cacheType, StringComparison.InvariantCultureIgnoreCase))
                return new NoCache();

            if (string.Equals("InMemoryCache", cacheType, StringComparison.InvariantCultureIgnoreCase))
                return new InMemoryCache();

            return new FileCache(cachePath);
        }

        public void Dispose()
        {
        }
    }
}
