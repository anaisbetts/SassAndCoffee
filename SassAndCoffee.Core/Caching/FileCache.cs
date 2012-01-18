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
        public const int NonceSize = 4;
        public const string DefaultCachePath = @".\.SassAndCoffeeCache\";
        private readonly string _cachePath;
        private byte[] _nonce = new byte[4];

        /// <summary>
        /// Initializes a new instance of the <see cref="FileCache"/> class.
        /// Defaults to [AppDomainBase]\.SassAndCoffeeCache\
        /// </summary>
        public FileCache()
            : this(null) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileCache"/> class.
        /// </summary>
        /// <param name="cachePath">The cache path. Defaults to [AppDomainBase]\.SassAndCoffeeCache\ if null.</param>
        public FileCache(string cachePath) {
            if (string.IsNullOrWhiteSpace(cachePath)) {
                cachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultCachePath);
            }
            // Normalize Path
            cachePath = new DirectoryInfo(cachePath).FullName;
            if (!cachePath.EndsWith("" + Path.DirectorySeparatorChar)
                && !cachePath.EndsWith("" + Path.AltDirectorySeparatorChar)) {
                cachePath += Path.DirectorySeparatorChar;
            }
            _cachePath = cachePath;
        }

        /// <summary>
        /// If available, returns the cached content for the requested resource. Returns false if not found.
        /// Must be thread safe per resource.
        /// </summary>
        /// <param name="resource">The resource requested.</param>
        /// <param name="result">The cached result. If null when returning true, interpreted as "Not Found".</param>
        /// <returns></returns>
        public bool TryGet(string resource, out ContentResult result) {
            var file = new FileInfo(GetCacheForResource(resource));
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
        /// Sets the cached content for the specified resource.
        /// Need not be thread safe.
        /// </summary>
        /// <param name="resource">The resource requested.</param>
        /// <param name="result">The content for that resource.</param>
        public void Set(string resource, ContentResult result) {
            var file = new FileInfo(GetCacheForResource(resource));
            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Delete)) {
                formatter.Serialize(stream, result);
            }
        }

        /// <summary>
        /// Invalidates the cached content for the specified resource.
        /// Need not be thread safe.
        /// </summary>
        /// <param name="resource">The cached resource to invalidate.</param>
        public void Invalidate(string resource) {
            var file = new FileInfo(GetCacheForResource(resource));
            if (file.Exists)
                file.Delete();
        }

        /// <summary>
        /// Clears the cache.
        /// Need not be thread safe.
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
        /// Need not be thread safe.
        /// </summary>
        public void Initialize() {
            Clear();
        }

        private void SetNonce() {
            /* This random nonce keeps us from accidentally using a stale cache across different runs of the app.
             * There's no efficient way for us to know if source files changed while we weren't running.
             */
            using (var rng = new RNGCryptoServiceProvider()) {
                var temp = new byte[NonceSize];
                rng.GetBytes(temp);
                _nonce = temp;
            }
        }

        private string GetCacheForResource(string resource) {
            // Since we're on Windows...
            var fileInfo = new FileInfo(resource);
            var normalizedPath = fileInfo.FullName.ToUpperInvariant();

            // Grab a local copy in case it changes under us (Clear)
            var nonce = _nonce;
            // Create a new instance here each time for thread safety of reads
            using (var hmac = new HMACSHA1(nonce)) {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(normalizedPath));
                var fileName = new SoapHexBinary(hash).ToString();
                return Path.Combine(_cachePath, fileName);
            }
        }
    }
}
