using System.Web.Hosting;

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
            var cacheVPath = ConfigurationManager.AppSettings["SassAndCoffee.CachePath"];
            if (string.IsNullOrEmpty(cacheVPath)) {
                cacheVPath = "~/App_Data/_FileCache";
            }

            context.PostResolveRequestCache += (o, e) => {
                var app = o as HttpApplication;
                _compiler = _compiler ?? initializeCompilerFromSettings(cacheType, MapPath(cacheVPath));
                _handler = _handler ?? new CompilableFileHandler(_compiler);

                if (!_compiler.CanCompile(app.Request.Path)) {
                    return;
                }

                app.Context.RemapHandler(_handler);
            };
        }

        public string MapPath(string path)
        {
            return HttpContext.Current.Server.MapPath(path);
        }

        IContentCompiler initializeCompilerFromSettings(string cacheType, string cachePath)
        {
            if (string.Equals(cacheType, "NoCache", StringComparison.InvariantCultureIgnoreCase)) {
                // NoCache
                return new ContentCompiler(this, new NoCache());
            } else if (string.Equals(cacheType, "InMemoryCache", StringComparison.InvariantCultureIgnoreCase)) {
                // InMemoryCache
                return new ContentCompiler(this, new InMemoryCache());
            } else {
                // FileCache
                if (!Directory.Exists(cachePath)) {
                    Directory.CreateDirectory(cachePath);
                }

                return new ContentCompiler(this, new FileCache(cachePath));
            }
        }

        public void Dispose()
        {
        }
    }
}
