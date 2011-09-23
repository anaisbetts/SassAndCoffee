using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Scripting;

namespace SassAndCoffee.Core.Compilers {
    public class VirtualFilePAL: PlatformAdaptationLayer {
        private enum PathType {
            None,
            Resource,
            Virtual,
            Cache
        }

        private static readonly Regex _filenameHeuristic = new Regex(@"/[^./]+(\.[^.]+)+$", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.RightToLeft|RegexOptions.Singleline|RegexOptions.ExplicitCapture);
        private static readonly Regex _pathCanonizer = new Regex(@"[/\\](\.(?=$|[/\\])|[^/\\]+[/\\]\.\.)", RegexOptions.CultureInvariant|RegexOptions.Compiled|RegexOptions.ExplicitCapture);
        private static readonly Regex _resourcePathEscaper = new Regex(@"\b(?=\d)|[/\\]", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.ExplicitCapture);
        private static readonly Regex _resourcePathTrimmer = new Regex(@"^.*(?=(\.[^.]+){2})", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.ExplicitCapture);
        private static readonly Regex _pathAbsolute = new Regex(@"^((?<drive>[a-z]):)?[/\\]", RegexOptions.Compiled|RegexOptions.IgnoreCase|RegexOptions.CultureInvariant|RegexOptions.ExplicitCapture);
//        private static readonly Regex _pathAnalyzer = new Regex(@"^(?<path>(?<absolute>([a-z]:)?[/\\])([^/\\]+[/\\])*)(?<file>[^/\\]+)$", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase|RegexOptions.ExplicitCapture);
        private static readonly ICollection<string> _resourceDirectories = new HashSet<string>(typeof(VirtualFilePAL).Assembly.GetManifestResourceNames().Select(s => _resourcePathTrimmer.Match(s).Value));

/*        internal static string JoinPaths(string basePath, string relativePath) {
            Match relativeMatch = _pathAnalyzer.Match(relativePath);
            if (relativeMatch.Groups["absolute"].Success) {
                return CanonizePath(relativePath);
            }
            return CanonizePath(_pathAnalyzer.Match(basePath).Groups["path"].Value+relativePath);
        } */

        internal static string CanonizePath(string path) {
            return _pathCanonizer.Replace(path, "");
        }

        private readonly string _cachePath;
        private ICompilerFile _compilerFile;
        private string _currentDirectory;

        public VirtualFilePAL() {
            _cachePath = Path.Combine(Path.GetTempPath(), "sass_cache", Guid.NewGuid().ToString("N"));
        }

        public override string CurrentDirectory {
            get {
                return _currentDirectory ?? @"C:\";
            }
            set {
                _currentDirectory = GetFullPath(value);
            }
        }

        internal static string GetDriveInternal(string path) {
            return _pathAbsolute.Match(path).Groups["drive"].Value;
        }

        public override void CreateDirectory(string path) {
            switch (ResolvePath(ref path)) {
            case PathType.Cache:
                base.CreateDirectory(path);
                break;
            default:
                throw new NotSupportedException();
            }
        }

        public override void DeleteDirectory(string path, bool recursive) {
            switch (ResolvePath(ref path)) {
            case PathType.Cache:
                base.DeleteDirectory(path, recursive);
                break;
            default:
                throw new NotSupportedException();
            }
        }

        public override void DeleteFile(string path, bool deleteReadOnly) {
            switch (ResolvePath(ref path)) {
            case PathType.Cache:
                base.DeleteFile(path, deleteReadOnly);
                break;
            default:
                throw new NotSupportedException();
            }
        }

        public override bool DirectoryExists(string path) {
            switch (ResolvePath(ref path)) {
            case PathType.Resource:
                return _resourceDirectories.Contains(path);
            case PathType.Virtual:
                return !(_filenameHeuristic.IsMatch(path) || _compilerFile.GetRelativeFile(path).Exists);
            case PathType.Cache:
                return base.DirectoryExists(path);
            default:
                throw new NotSupportedException();
            }
        }

        public override bool FileExists(string path) {
            switch (ResolvePath(ref path)) {
            case PathType.Resource:
                using (Stream stream = typeof(VirtualFilePAL).Assembly.GetManifestResourceStream(path)) {
                    return stream != null;
                }
            case PathType.Virtual:
                return _compilerFile.GetRelativeFile(path).Exists;
            case PathType.Cache:
                return base.FileExists(path);
            default:
                throw new NotSupportedException();
            }
        }

        public override string[] GetFileSystemEntries(string path, string searchPattern, bool includeFiles, bool includeDirectories) {
            switch (ResolvePath(ref path)) {
            case PathType.Cache:
                return base.GetFileSystemEntries(path, searchPattern, includeFiles, includeDirectories);
            default:
                throw new NotSupportedException();
            }
        }

        public override string GetFullPath(string path) {
            Match match = _pathAbsolute.Match(path);
            if (match.Success) {
                if (!match.Groups["drive"].Success) {
                    path = GetDriveInternal(path)+':'+path;
                }
            } else {
                path = CombinePaths(CurrentDirectory, path);
            }
            return CanonizePath(path);
        }

        public override void MoveFileSystemEntry(string sourcePath, string destinationPath) {
            if ((ResolvePath(ref sourcePath) == PathType.Cache) && (ResolvePath(ref destinationPath) == PathType.Cache)) {
                base.MoveFileSystemEntry(sourcePath, destinationPath);
            }
            throw new NotSupportedException();
        }

        public override Stream OpenInputFileStream(string path) {
            return OpenInputFileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share) {
            bool readOnly = (mode == FileMode.Open) && (access == FileAccess.Read);
            switch (ResolvePath(ref path)) {
            case PathType.Resource:
                if (readOnly) {
                    return typeof(VirtualFilePAL).Assembly.GetManifestResourceStream(path);
                }
                break;
            case PathType.Virtual:
                if (readOnly) {
                    return _compilerFile.GetRelativeFile(path).Open();
                }
                break;
            case PathType.Cache:
                return File.Open(Path.Combine(_cachePath, path), mode, access, share);
            }
            throw new NotSupportedException();
        }

        public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) {
            return OpenInputFileStream(path, mode, access, share);
        }

        public override Stream OpenOutputFileStream(string path) {
            switch (ResolvePath(ref path)) {
            case PathType.Cache:
                return base.OpenOutputFileStream(path);
            default:
                throw new NotSupportedException();
            }
        }

        internal IDisposable SetCompilerFile(ICompilerFile file) {
            if (file == null) {
                throw new ArgumentNullException("file");
            }
            Debug.Assert(_compilerFile == null);
            _compilerFile = file;
            _currentDirectory = Path.GetDirectoryName(@"V:"+file.Name);
            Directory.CreateDirectory(_cachePath);
            return Disposable.Create(delegate {
                                         _compilerFile = null;
                                         _currentDirectory = null;
                                         Directory.Delete(_cachePath, true);
                                     });
        }

        private PathType ResolvePath(ref string path) {
            string[] parts = GetFullPath(path).Split(':');
            if (parts.Length == 2) {
                path = parts[1];
                switch (parts[0]) {
                case "R":
                    path = "SassAndCoffee.Core"+_resourcePathEscaper.Replace(parts[1], m => (m.Length == 0) ? "_" : ".");
                    return PathType.Resource;
                case "V":
                    path = _compilerFile.GetRelativeFile(parts[1].Replace('\\', '/')).Name;
                    return PathType.Virtual;
                case "C":
                    path = _cachePath+parts[1].Replace('/', '\\');
                    return PathType.Cache;
                }
            }
            return PathType.None;
        }
    }
}
