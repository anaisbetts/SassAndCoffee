﻿namespace SassAndCoffee.AspNet
{
    using System;
    using System.IO;
    using System.Web;

    using SassAndCoffee.Core;

    public class CompilableFileHandler : IHttpHandler
    {
        private readonly IContentCompiler _contentCompiler;

        public CompilableFileHandler(IContentCompiler contentCompiler)
        {
            _contentCompiler = contentCompiler;
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            var fi = new FileInfo(context.Request.PhysicalPath);
            var requestedFileName = fi.FullName;
            if (fi.Exists)
            {
                BuildHeaders(context.Response, this._contentCompiler.GetOutputMimeType(requestedFileName), fi.LastWriteTimeUtc);
                context.Response.WriteFile(requestedFileName);
                return;
            }

            var compilationResult = this._contentCompiler.GetCompiledContent(context.Request.Path);
            if (compilationResult.Compiled == false)
            {
                context.Response.StatusCode = 404;
                return;
            }

            BuildHeaders(context.Response, compilationResult.MimeType, compilationResult.SourceLastModifiedUtc);
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