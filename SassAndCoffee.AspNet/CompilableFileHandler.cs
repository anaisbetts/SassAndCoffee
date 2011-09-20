using System.IO.Compression;

namespace SassAndCoffee.AspNet
{
    using System;
    using System.Web;

    using SassAndCoffee.Core;

    public class CompilableFileHandler : IHttpHandler
    {
        readonly IContentCompiler _contentCompiler;

        public CompilableFileHandler(IContentCompiler contentCompiler)
        {
            _contentCompiler = contentCompiler;
        }

        public bool IsReusable {
            get {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            VirtualPathCompilerFile file = new VirtualPathCompilerFile(context.Request.Path);
            var compilationResult = _contentCompiler.GetCompiledContent(file);
            if (compilationResult.Compiled == false) {
                context.Response.StatusCode = 404;
                return;
            }

            BuildHeaders(context.Response, compilationResult.MimeType, compilationResult.SourceLastModifiedUtc);
            context.Response.Write(compilationResult.Contents);
        }

        static void BuildHeaders(HttpResponse response, string mimeType, DateTime lastModified)
        {
            response.StatusCode = 200;
            response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
            response.AddHeader("content-encoding", "gzip");
            response.Cache.VaryByHeaders["Accept-Encoding"] = true;
            response.AddHeader("ETag", lastModified.Ticks.ToString("x"));
            response.AddHeader("Content-Type", mimeType);
            response.AddHeader("Content-Disposition", "inline");
            response.AddHeader("Last-Modified", lastModified.ToString("R"));
        }
    }
}
