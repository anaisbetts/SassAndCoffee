namespace SassAndCoffee.AspNet {
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Web;
    using SassAndCoffee.Core.Pooling;
    using SassAndCoffee.Core.CoffeeScript;
    using SassAndCoffee.Core.Uglify;
    using SassAndCoffee.Core;

    public class JavaScriptHandler : IHttpHandler, IDisposable {

        private IInstanceProvider<IJavaScriptRuntime> _jsRuntimeProvider;
        private Pool<IJavaScriptCompiler, JavaScriptCompilerProxy> _coffeeCompilerPool;
        private Pool<IJavaScriptCompiler, JavaScriptCompilerProxy> _uglifyCompilerPool;

        public JavaScriptHandler() {
            _jsRuntimeProvider = new InstanceProvider<IJavaScriptRuntime>(
                () => new IEJavaScriptRuntime());
            _coffeeCompilerPool = new Pool<IJavaScriptCompiler, JavaScriptCompilerProxy>(
                () => new CoffeeScriptCompiler(_jsRuntimeProvider));
            _uglifyCompilerPool = new Pool<IJavaScriptCompiler, JavaScriptCompilerProxy>(
                () => new UglifyCompiler(_jsRuntimeProvider));
        }

        public bool IsReusable { get { return true; } }

        public void ProcessRequest(HttpContext context) {
            var request = context.Request;
            var response = context.Response;
            bool uglify = false;

            var fileRoot = GetRequestRoot(request.PhysicalPath);
            if (fileRoot == null) {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            var minIndex = fileRoot.LastIndexOf(".min", StringComparison.OrdinalIgnoreCase);
            if(minIndex > 0){
                fileRoot = fileRoot.Substring(0, minIndex);
                uglify = true;
            }

            string result = null;

            // Read in coffeescript source if found
            var coffeeFile = fileRoot + ".coffee";
            if (File.Exists(coffeeFile)) {
                using (var coffeeCompiler = _coffeeCompilerPool.GetInstance()) {
                    var coffeeSource = File.ReadAllText(coffeeFile);
                    result = coffeeCompiler.Compile(coffeeSource);
                }
            }

            // Read in javascript source if found
            if (result == null && uglify) {
                var jsToMinify = fileRoot + ".js";
                if (File.Exists(jsToMinify)) {
                    result = File.ReadAllText(jsToMinify);
                }
            }

            // Uglify
            if (result != null && uglify) {
                using (var compiler = _uglifyCompilerPool.GetInstance()) {
                    result = compiler.Compile(result);
                }
            }

            if (result == null) {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "text/javascript; charset=utf-8";
            response.Write(result);
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
                if (_coffeeCompilerPool != null) {
                    _coffeeCompilerPool.Dispose();
                    _coffeeCompilerPool = null;
                }
                if (_uglifyCompilerPool != null) {
                    _uglifyCompilerPool.Dispose();
                    _uglifyCompilerPool = null;
                }
            }
        }
    }
}

