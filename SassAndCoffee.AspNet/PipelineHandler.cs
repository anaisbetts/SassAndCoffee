namespace SassAndCoffee.AspNet {
    using System;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;
    using SassAndCoffee.Core;

    public class PipelineHandler : IHttpHandler {
        public const string CacheProfileName = "SassAndCoffeeCacheSettings";

        private IContentPipeline _pipeline;
        private OutputCacheProfile _cacheProfile;

        public bool IsReusable { get { return true; } }

        public PipelineHandler(IContentPipeline pipeline) {
            _pipeline = pipeline;

            var settings = WebConfigurationManager.GetWebApplicationSection(
                "system.web/caching/outputCacheSettings") as OutputCacheSettingsSection;

            _cacheProfile = settings.OutputCacheProfiles[CacheProfileName] ?? CreateDefaultCacheProfile();
        }

        public void ProcessRequest(HttpContext context) {
            if (context == null)
                throw new ArgumentNullException("context");

            var request = context.Request;
            var response = context.Response;
            var result = _pipeline.ProcessRequest(request.PhysicalPath);

            if (result == null) {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            if (_cacheProfile.Enabled) {
                SetCachePolicy(response, _cacheProfile.Location);
                response.Cache.SetMaxAge(TimeSpan.FromSeconds(_cacheProfile.Duration));
                response.Cache.SetExpires(DateTime.UtcNow.AddSeconds(_cacheProfile.Duration));

                if (_cacheProfile.NoStore)
                    response.Cache.SetNoStore();

                // No SQL dependency, VaryByControl, or VaryByCustom support for now
                SetVaryBy(_cacheProfile.VaryByContentEncoding, encoding => response.Cache.VaryByContentEncodings[encoding] = true);

                if (_cacheProfile.VaryByHeader == "*") {
                    response.Cache.VaryByHeaders.VaryByUnspecifiedParameters();
                } else {
                    SetVaryBy(_cacheProfile.VaryByHeader, header => response.Cache.VaryByHeaders[header] = true);
                }

                if (_cacheProfile.VaryByParam != "*") {
                    response.Cache.SetOmitVaryStar(true);
                    if (string.IsNullOrWhiteSpace(_cacheProfile.VaryByParam)
                        || _cacheProfile.VaryByParam == "none") {
                        response.Cache.VaryByParams.IgnoreParams = true;
                    } else {
                        SetVaryBy(_cacheProfile.VaryByParam, param => response.Cache.VaryByParams[param] = true);
                    }
                }

                if (result.CacheInvalidationFileList.Any()) {
                    response.AddFileDependencies(result.CacheInvalidationFileList.ToArray());
                    response.Cache.SetLastModifiedFromFileDependencies();
                    response.Cache.SetETagFromFileDependencies();
                }
            } else {
                response.Cache.SetCacheability(HttpCacheability.NoCache);
            }

            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = result.MimeType;
            response.Write(result.Content);
        }

        private static void SetVaryBy(string list, Action<string> action) {
            if (string.IsNullOrWhiteSpace(list))
                return;

            var trimmedItems = list
                .Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());

            foreach (var item in trimmedItems) {
                action(item);
            }
        }

        private static void SetCachePolicy(HttpResponse response, OutputCacheLocation outputCacheLocation) {
            switch (outputCacheLocation) {
                case OutputCacheLocation.Any:
                    response.Cache.SetCacheability(HttpCacheability.Public);
                    break;
                case OutputCacheLocation.Client:
                    response.Cache.SetCacheability(HttpCacheability.Private);
                    response.Cache.SetNoServerCaching();
                    break;
                case OutputCacheLocation.Downstream:
                    response.Cache.SetCacheability(HttpCacheability.Public);
                    response.Cache.SetNoServerCaching();
                    break;
                case OutputCacheLocation.None:
                    response.Cache.SetCacheability(HttpCacheability.NoCache);
                    response.Cache.SetNoServerCaching();
                    break;
                case OutputCacheLocation.Server:
                    response.Cache.SetCacheability(HttpCacheability.Server);
                    break;
                case OutputCacheLocation.ServerAndClient:
                    response.Cache.SetCacheability(HttpCacheability.ServerAndPrivate);
                    break;
            }
        }

        private static OutputCacheProfile CreateDefaultCacheProfile() {
            return new OutputCacheProfile(CacheProfileName) {
                Duration = 3600, // one hour
                Location = OutputCacheLocation.Any,
                VaryByHeader = "Accept-Encoding",
                VaryByParam = "none",
                Enabled = true,
            };
        }
    }
}
