using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace SassAndCoffee
{
    public class CompilableFileModule : IHttpModule
    {
        Dictionary<string, IHttpHandler> _handlers;

        public void Init(HttpApplication context)
        {
            var compilers = new[] {
                new CoffeeScriptFileCompiler(),
            };

            _handlers = new Dictionary<string, IHttpHandler>();
            foreach (var compiler in compilers) {
                compiler.Init(context);
                _handlers[compiler.OutputFileExtension] = new CompilableFileHandler(compiler);
            }

            context.PostResolveRequestCache += (o, e) => {
                var app = o as HttpApplication;
                string ext = Path.GetExtension(app.Request.PhysicalPath.ToLowerInvariant());
                if (!_handlers.ContainsKey(ext))
                {
                    return;
                }

                app.Context.RemapHandler(_handlers[ext]);
            };
        }

        public void Dispose()
        {
        }
    }
}
