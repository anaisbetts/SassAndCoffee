using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace SassAndCoffee
{
    public class CompilableFileHandler : IHttpHandler
    {
        ISimpleFileCompiler _compiler;
        public bool IsReusable {
            get { return true; }
        }

        public CompilableFileHandler(ISimpleFileCompiler compiler)
        {
            _compiler = compiler;
        }

        public void ProcessRequest(HttpContext context)
        {
            // If the output file exists for some reason (i.e. it's precompiled), 
            // just serve it up
            var fi = new FileInfo(context.Request.PhysicalPath);
            if (fi.Exists) {
                transmitify(context.Response, fi);
                return;
            }

            // Look for a file with the same name, but with one of the input 
            // extensions we're interested in
            foreach (var ext in _compiler.InputFileExtensions) {
                fi = new FileInfo(Path.Combine(fi.DirectoryName,
                    Path.GetFileNameWithoutExtension(fi.FullName) + ext));

                if (fi.Exists) {
                    break;
                }
            }

            // No file still? Bummer.
            if (!fi.Exists) {
                context.Response.StatusCode = 404;
                return;
            }

            // Does the cached version of the file exist? Serve it up!
            var outFile = new FileInfo(getOutputFilePath(context, fi.FullName));
            if (!outFile.Exists) {
                using (var of = File.Create(outFile.FullName)) {
                    var buf = Encoding.UTF8.GetBytes(_compiler.ProcessFileContent(fi.FullName));
                    of.Write(buf, 0, buf.Length);
                }
            }

            // Finally! Serve it up
            transmitify(context.Response, outFile);
        }

        void transmitify(HttpResponse response, FileInfo fi)
        {
            response.StatusCode = 200;
            response.AddHeader("ETag", fi.LastWriteTimeUtc.Ticks.ToString("x"));
            response.AddHeader("Content-Type", _compiler.OutputMimeType);
            response.AddHeader("Content-Disposition", "inline");
            response.AddHeader("Last-Modified", fi.LastWriteTimeUtc.ToString("R"));
            response.WriteFile(fi.FullName);
        }

        string getOrCreateCachePath(HttpContext context)
        {
            var appData = new DirectoryInfo(Path.Combine(context.Request.PhysicalApplicationPath, "App_Data"));
            var cacheDir = appData.GetDirectories().FirstOrDefault(x => x.Name.ToLowerInvariant() == "_filecache") ?? appData.CreateSubdirectory("_FileCache");
            return cacheDir.FullName;
        }

        string getOutputFilePath(HttpContext context, string inputFileName)
        {
            var fi = new FileInfo(inputFileName);

            string name = String.Format("{0:yyyyMMddHHmmss}-{1}{2}",
                fi.LastWriteTimeUtc,
                Path.GetFileNameWithoutExtension(inputFileName),
                _compiler.OutputFileExtension);

            return Path.Combine(getOrCreateCachePath(context), name);
        }
    }
}
