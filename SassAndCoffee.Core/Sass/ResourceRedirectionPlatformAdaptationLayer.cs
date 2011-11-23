namespace SassAndCoffee.Core.Sass {
    using System;
    using System.IO;
    using System.Reflection;
    using Microsoft.Scripting;

    public class ResourceRedirectionPlatformAdaptationLayer : PlatformAdaptationLayer {

        public Action<string> OnOpenInputFileStream { get; set; }

        public override bool FileExists(string path) {
            var assembly = Assembly.GetExecutingAssembly();
            var type = typeof(ResourceRedirectionPlatformAdaptationLayer);
            var resourceName = PathToResourceName(path);

            try {
                if (assembly.GetManifestResourceStream(type, resourceName) != null) {
                    return true;
                }
            } catch { }

            return base.FileExists(path);
        }

        public override Stream OpenInputFileStream(string path) {
            var assembly = Assembly.GetExecutingAssembly();
            var type = typeof(ResourceRedirectionPlatformAdaptationLayer);
            var resourceName = PathToResourceName(path);

            try {
                return assembly.GetManifestResourceStream(type, resourceName);
            } catch { }

            if (OnOpenInputFileStream != null) OnOpenInputFileStream(path);
            return base.OpenInputFileStream(path);
        }

        private string PathToResourceName(string path) {
            // This is kind of a hack, but I couldn't think of anything better worth the effort.
            return path                
                .Replace("1.9.1", "_1._9._1")
                .Replace('\\', '.')
                .Replace('/', '.')
                .Replace(@"R:.{345ED29D-C275-4C64-8372-65B06E54F5A7}", "")
                .TrimStart('.');
        }
    }
}
