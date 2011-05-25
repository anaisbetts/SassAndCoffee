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
        Dictionary<ISimpleFileCompiler, IHttpHandler> _handlers;

        public void Init(HttpApplication context)
        {
            var compilers = new ISimpleFileCompiler[] {
                new MinifyingFileCompiler(),
                new CoffeeScriptFileCompiler(),
                new SassFileCompiler(),
            };

            _handlers = new Dictionary<ISimpleFileCompiler, IHttpHandler>();
            foreach (var compiler in compilers) {
                compiler.Init(context);
                _handlers[compiler] = new CompilableFileHandler(compiler);
            }

            context.PostResolveRequestCache += (o, e) => {
                var app = o as HttpApplication;
                string path = app.Request.PhysicalPath.ToLowerInvariant();
                var compiler = _handlers.Keys.FirstOrDefault(x => path.EndsWith(x.OutputFileExtension));

                if (compiler == null) {
                    return;
                }

                app.Context.RemapHandler(_handlers[compiler]);
            };
        }

        public void Dispose()
        {
        }
    }
}
