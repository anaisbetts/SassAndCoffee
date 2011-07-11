namespace SassAndCoffee.AspNet
{
    using System;
    using System.IO;
    using System.Web;

    using SassAndCoffee.Core;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.Reflection;
    using System.Collections.Specialized;

    public class CompilableFileHandler : IHttpHandler
    {
        readonly ICompilerHost _host;
        readonly NameValueCollection _mimeMap = new NameValueCollection()
        {
            {".css", "text/css"},
            {".js", "text/javascript"}
        };

        public CompilableFileHandler(ICompilerHost host)
        {
            _host = host;
        }

        public bool IsReusable {
            get {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            var fi = new FileInfo(context.Request.PhysicalPath);
            var requestedFileName = fi.FullName;

            if (fi.Exists) {
                BuildHeaders(context.Response, _mimeMap[Path.GetExtension(requestedFileName)], fi.LastWriteTimeUtc);
                context.Response.WriteFile(requestedFileName);
                return;
            }

            var compilationResult = this._host.Compiler.GetCompiledContent(requestedFileName);
            if (compilationResult.Compiled == false) {
                context.Response.StatusCode = 404;
                return;
            }

            BuildHeaders(context.Response, _mimeMap[Path.GetExtension(requestedFileName)], compilationResult.SourceLastModifiedUtc);
            context.Response.Write(compilationResult.Contents);
        }

        static void BuildHeaders(HttpResponse response, string mimeType, DateTime lastModified)
        {
            response.StatusCode = 200;
            response.AddHeader("ETag", lastModified.Ticks.ToString("x"));
            response.AddHeader("Content-Type", mimeType);
            response.AddHeader("Content-Disposition", "inline");
            response.AddHeader("Last-Modified", lastModified.ToString("R"));
        }
    }
}
