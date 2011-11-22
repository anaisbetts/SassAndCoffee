namespace SassAndCoffee.AspNet {
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Web;
    using SassAndCoffee.Core.Pooling;
    using SassAndCoffee.Core.Sass;

    public class SassHandler : IHttpHandler, IDisposable {

        private Pool<ISassCompiler, SassCompilerProxy> _compilerPool =
            new Pool<ISassCompiler, SassCompilerProxy>(() => new SassCompiler());

        public bool IsReusable { get { return true; } }

        public void ProcessRequest(HttpContext context) {
            var request = context.Request;
            var response = context.Response;

            var fileRoot = GetRequestRoot(request.PhysicalPath);
            if (fileRoot == null) {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            var fileSource = FindFileFromRoot(fileRoot);
            if (fileSource == null) {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            string result = null;
            using (var compiler = _compilerPool.GetInstance()) {
                result = compiler.Compile(fileSource);
            }
            if (result == null) {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "text/css; charset=utf-8";
            response.Write(result);
        }

        private string FindFileFromRoot(string fileRoot) {
            var fileName = fileRoot + ".scss";
            if (File.Exists(fileName))
                return fileName;

            fileName = fileRoot + ".sass";
            if (File.Exists(fileName))
                return fileName;

            return null;
        }

        private string GetRequestRoot(string physicalPath) {
            var lastDot = physicalPath.LastIndexOf('.');
            if (lastDot < 0)
                return null;

            return physicalPath.Substring(0, lastDot);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_compilerPool != null) {
                    _compilerPool.Dispose();
                    _compilerPool = null;
                }
            }
        }
    }
}
