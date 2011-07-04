namespace SassAndCoffee.AspNet
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

//namespace SassAndCoffee.AspNet
//{
//    using System;
//    using System.IO;
//    using System.Linq;
//    using System.Text;
//    using System.Threading;
//    using System.Web;

//    using SassAndCoffee.Core;
//    using SassAndCoffee.Core.Compilers;
//    using SassAndCoffee.Core.Extensions;

//    public class CompilableFileHandler : IHttpHandler
//    {
//        ISimpleFileCompiler _compiler;
//        public bool IsReusable {
//            get { return true; }
//        }

//        static object _gate = 42;

//        public CompilableFileHandler(ISimpleFileCompiler compiler)
//        {
//            _compiler = compiler;
//        }

//        public void ProcessRequest(HttpContext context)
//        {
//            // If the output file exists for some reason (i.e. it's precompiled), 
//            // just serve it up
//            var fi = new FileInfo(context.Request.PhysicalPath);
//            if (fi.Exists) {
//                transmitify(context.Response, fi);
//                return;
//            }

//            fi = new FileInfo(_compiler.FindInputFileGivenOutput(fi.FullName) ?? String.Empty);

//            // No file still? Bummer.
//            if (!fi.Exists) {
//                context.Response.StatusCode = 404;
//                return;
//            }

//            // Does the cached version of the file not exist? Build it!
//            var outFile = new FileInfo(getOutputFilePath(context, fi.FullName));
//            if (!outFile.Exists) {
//                byte[] buf;

//                try {
//                    buf = Encoding.UTF8.GetBytes(_compiler.ProcessFileContent(fi.FullName));
//                } catch (Exception ex) {
//                    buf = Encoding.UTF8.GetBytes(ex.Message);
//                }

//                int retries = 3;
//                while (retries > 0) {
//                    try {
//                        using (var of = File.Create(outFile.FullName)) {
//                            of.Write(buf, 0, buf.Length);
//                            break;
//                        }
//                    } catch {
//                        retries--;
//                        if (retries == 0) {
//                            throw;
//                        }
//                        Thread.Sleep(1 * 1000);
//                    }
//                }
//            }

//            // Finally! Serve it up
//            transmitify(context.Response, outFile);
//        }

//        void transmitify(HttpResponse response, FileInfo fi)
//        {
//            response.StatusCode = 200;
//            response.AddHeader("ETag", fi.LastWriteTimeUtc.Ticks.ToString("x"));
//            response.AddHeader("Content-Type", _compiler.OutputMimeType);
//            response.AddHeader("Content-Disposition", "inline");
//            response.AddHeader("Last-Modified", fi.LastWriteTimeUtc.ToString("R"));
//            response.WriteFile(fi.FullName);
//        }

//        string getOrCreateCachePath(HttpContext context)
//        {
//            var appData = new DirectoryInfo(Path.Combine(context.Request.PhysicalApplicationPath, "App_Data"));

//            if (!appData.Exists) {
//                appData.Create();
//            }

//            var cacheDir = appData.GetDirectories().FirstOrDefault(x => x.Name.ToLowerInvariant() == "_filecache") ?? appData.CreateSubdirectory("_FileCache");
//            return cacheDir.FullName;
//        }

//        string getOutputFilePath(HttpContext context, string inputFileName)
//        {
//            var fi = new FileInfo(inputFileName);
//            var token = _compiler.GetFileChangeToken(inputFileName);

//            string name = String.Format("{0:yyyyMMddHHmmss}-{1}-{2}{3}",
//                fi.LastWriteTimeUtc, token,
//                Path.GetFileNameWithoutExtension(inputFileName),
//                _compiler.OutputFileExtension);

//            return Path.Combine(getOrCreateCachePath(context), name);
//        }
//    }
//}