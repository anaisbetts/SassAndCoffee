namespace SassAndCoffee.AspNet
{
    using System.Web;

    using SassAndCoffee.Core;
    using SassAndCoffee.Core.Caching;

    public class CompilableFileModule : IHttpModule, ICompilerHost
    {
        private IContentCompiler _compiler;

        private IHttpHandler _handler;

        private HttpApplication _application;

        public void Init(HttpApplication context)
        {
            this._application = context;
            this._compiler = ContentCompiler.WithAllCompilers(this, new InMemoryCache());
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
