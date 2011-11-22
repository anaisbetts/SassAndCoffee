namespace SassAndCoffee.Core.Sass {
    using System.IO;
    using System.Reflection;
    using Microsoft.Scripting;

    public class ResourceRedirectionPlatformAdaptationLayer : PlatformAdaptationLayer {

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

            return base.OpenInputFileStream(path);
        }

        private string PathToResourceName(string path) {
            // This is kind of a hack, but I couldn't think of anything better worth the effort.
            return path
                .Replace("1.9.1", "_1._9._1")
                .Replace('\\', '.')
                .Replace('/', '.')
                .Replace("R:", "")
                .TrimStart('.');
        }
    }
}
