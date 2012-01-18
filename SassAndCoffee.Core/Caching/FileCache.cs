namespace SassAndCoffee.Core {
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Caches files to disk.
    /// </summary>
    public class FileCache : IContentCache {
        public const string DefaultCachePath = @".\SassAndCoffeeCache\";
        private readonly string _cachePath;
        private byte[] _nonce = new byte[4];

        /// <summary>
        /// Initializes a new instance of the <see cref="FileCache"/> class.
        /// </summary>
        public FileCache()
            : this(null) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileCache"/> class.
        /// </summary>
        /// <param name="cachePath">The cache path.</param>
        public FileCache(string cachePath) {
            if (string.IsNullOrWhiteSpace(cachePath)) {
                _cachePath = DefaultCachePath;
            } else {
                if (!cachePath.EndsWith("" + Path.DirectorySeparatorChar)
                    && !cachePath.EndsWith("" + Path.AltDirectorySeparatorChar)) {
                    cachePath += Path.DirectorySeparatorChar;
                }
                _cachePath = cachePath;
            }
        }

        /// <summary>
        /// Tries to get a cached copy of the requested path.  Returns false if not found.
        /// </summary>
        /// <param name="path">The path requested.</param>
        /// <param name="result">The cached result. Null is a valid value.</param>
        /// <returns></returns>
        public bool TryGet(string path, out ContentResult result) {
            var file = new FileInfo(GetCacheForPath(path));
            if (file.Exists) {
                var formatter = new BinaryFormatter();
                using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete)) {
                    result = formatter.Deserialize(stream) as ContentResult;
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Sets the cached result for the specified path.
        /// </summary>
        /// <param name="path">The path requested.</param>
        /// <param name="result">The result for that path.</param>
        public void Set(string path, ContentResult result) {
            var file = new FileInfo(GetCacheForPath(path));
            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Delete)) {
                formatter.Serialize(stream, result);
            }
        }

        /// <summary>
        /// Invalidates the specified cached path.
        /// </summary>
        /// <param name="path">The cached path to invalidate.</param>
        public void Invalidate(string path) {
            var file = new FileInfo(GetCacheForPath(path));
            if (file.Exists)
                file.Delete();
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear() {
            SetNonce();

            var directory = new DirectoryInfo(_cachePath);
            if (!directory.Exists)
                directory.Create();

            foreach (var item in directory.EnumerateFileSystemInfos()) {
                var cacheFile = item as FileInfo;
                var cacheDir = item as DirectoryInfo;

                if (cacheFile != null)
                    cacheFile.Delete();
                if (cacheDir != null)
                    cacheDir.Delete(true);
            }

            var cacheDirectory = new DirectoryInfo(_cachePath);
            if (cacheDirectory.EnumerateFileSystemInfos().Any()) {
                throw new Exception(string.Format("Directory \"{0}\" could not be cleared.", cacheDirectory.FullName));
            }
        }

        /// <summary>
        /// Initializes the cache. May throw exceptions and perform IO.
        /// Must be called before attempting to use the cache.
        /// </summary>
        public void Initialize() {
            Clear();
        }

        private void SetNonce() {
            /* This random nonce keeps us from accidentally using a stale cache across different runs of the app.
             * There's no efficient way for us to know if source files changed while we weren't running.
             */
            using (var rng = new RNGCryptoServiceProvider()) {
                rng.GetBytes(_nonce);
            }
        }

        private string GetCacheForPath(string path) {
            // TODO: Case insensitivity?
            // TODO: Performance?
            using (var hmac = new HMACSHA1(_nonce)) {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(path));
                var fileName = new SoapHexBinary(hash).ToString();
                return Path.Combine(_cachePath, fileName);
            }
        }
    }
}
