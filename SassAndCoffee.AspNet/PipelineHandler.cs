namespace SassAndCoffee.AspNet {
    using System;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using SassAndCoffee.Core.Pipeline;

    public class PipelineHandler : IHttpHandler, IDisposable {
        private IContentPipeline _pipeline;

        public bool IsReusable { get { return true; } }

        public PipelineHandler(IContentPipeline pipeline) {
            _pipeline = pipeline;
        }

        public void ProcessRequest(HttpContext context) {
            var request = context.Request;
            var response = context.Response;

            var result = _pipeline.ProcessRequest(request.PhysicalPath);

            if (result == null) {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            } else {
                // TODO: This is needed for kernel caching.  Is it worth it?
                //response.Cache.SetOmitVaryStar(true);
                if (result.CacheInvalidationFileList.Any()) {
                    response.AddFileDependencies(result.CacheInvalidationFileList.ToArray());
                }
                if (result.MaxAgeSeconds.HasValue) {
                    response.Cache.SetMaxAge(TimeSpan.FromSeconds(result.MaxAgeSeconds.Value));
                }
                response.Cache.SetCacheability(HttpCacheability.Public);
                response.Cache.SetLastModifiedFromFileDependencies();
                response.Cache.SetETagFromFileDependencies();
                response.ContentEncoding = Encoding.UTF8;
                response.ContentType = result.MimeType;
                response.Write(result.Content);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_pipeline != null) {
                    _pipeline.Dispose();
                    _pipeline = null;
                }
            }
        }
    }
}
