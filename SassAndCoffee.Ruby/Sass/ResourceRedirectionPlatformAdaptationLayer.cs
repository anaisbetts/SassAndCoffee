namespace SassAndCoffee.Ruby.Sass {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using Microsoft.Scripting;

    public class ResourceRedirectionPlatformAdaptationLayer : PlatformAdaptationLayer {

        public Action<string> OnOpenInputFileStream { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The reason for failure is irrelevant.")]
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

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The reason for failure is irrelevant.")]
        public override Stream OpenInputFileStream(string path) {
            var assembly = Assembly.GetExecutingAssembly();
            var type = typeof(ResourceRedirectionPlatformAdaptationLayer);
            var resourceName = PathToResourceName(path);

            try {
                return assembly.GetManifestResourceStream(type, resourceName);
            } catch { }

            if (OnOpenInputFileStream != null)
                OnOpenInputFileStream(GetFullPath(path));
            return base.OpenInputFileStream(path);
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share) {
            if (OnOpenInputFileStream != null)
                OnOpenInputFileStream(GetFullPath(path));
            return base.OpenInputFileStream(path, mode, access, share);
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) {
            if (OnOpenInputFileStream != null)
                OnOpenInputFileStream(GetFullPath(path));
            return base.OpenInputFileStream(path, mode, access, share, bufferSize);
        }

        private static string PathToResourceName(string path) {
            // This is kind of a hack, but I couldn't think of anything better worth the effort.
            return path
                .Replace("1.9.1", "_1._9._1")
                .Replace('\\', '.')
                .Replace('/', '.')
                .Replace(@"R:.{345ED29D-C275-4C64-8372-65B06E54F5A7}", "")
                .TrimStart('.');
        }

        /* These methods are disabled for safety.  We shouldn't need them. */

        public override void CreateDirectory(string path) {
            throw new NotImplementedException();
        }

        public override void DeleteDirectory(string path, bool recursive) {
            throw new NotImplementedException();
        }

        public override void DeleteFile(string path, bool deleteReadOnly) {
            throw new NotImplementedException();
        }

        public override void MoveFileSystemEntry(string sourcePath, string destinationPath) {
            throw new NotImplementedException();
        }

        public override void SetEnvironmentVariable(string key, string value) {
            throw new NotImplementedException();
        }

        public override Stream OpenOutputFileStream(string path) {
            throw new NotImplementedException();
        }
    }
}
