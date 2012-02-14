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
    public class FileMedium : IPersistentMedium {
        public const string DefaultCachePath = @".\.SassAndCoffeeCache\";
        private readonly string _cachePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileMedium"/> class.
        /// Defaults to [AppDomainBase]\.SassAndCoffeeCache\
        /// </summary>
        public FileMedium()
            : this(null) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileMedium"/> class.
        /// </summary>
        /// <param name="cachePath">The cache path. Defaults to [AppDomainBase]\.SassAndCoffeeCache\ if null.</param>
        public FileMedium(string cachePath) {
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
        /// Initializes this instance.  Must be called before using the cache.
        /// </summary>
        public void Initialize() {
            var directory = new DirectoryInfo(_cachePath);
            if (!directory.Exists) {
                directory.Create();
                return;
            }

            var formatter = new BinaryFormatter();
            foreach (var file in directory.EnumerateFiles()) {
                CachedContentResult fileData = null;
                try {
                    using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete)) {
                        fileData = formatter.Deserialize(stream) as CachedContentResult;
                    }
                } catch { }

                if (fileData == null || !fileData.ValidateHashes()) {
                    file.Delete();
                }
            }
        }

        /// <summary>
        /// If available, returns the cached content for the requested resource.
        /// Must be thread safe per key.
        /// </summary>
        /// <param name="key">The unique key for the resource requested.</param>
        /// <returns>The cached result, or null on cache miss.</returns>
        public CachedContentResult TryGet(string key) {
            var file = new FileInfo(GetCacheForKey(key));
            if (file.Exists) {
                var formatter = new BinaryFormatter();
                using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete)) {
                    return formatter.Deserialize(stream) as CachedContentResult;
                }
            }
            return null;
        }

        /// <summary>
        /// Sets the cached content for the specified key.
        /// Must be thread safe per key.
        /// </summary>
        /// <param name="key">The unique key for the resource requested.</param>
        /// <param name="result">The result to cache.</param>
        public void Set(string key, CachedContentResult result) {
            var file = new FileInfo(GetCacheForKey(key));
            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Delete)) {
                formatter.Serialize(stream, result);
            }
        }

        /// <summary>
        /// Removes the cached content for the specified resource.
        /// Must be thread safe per key.
        /// </summary>
        /// <param name="key">The unique key of the cached resource to remove.</param>
        public void Remove(string key) {
            var file = new FileInfo(GetCacheForKey(key));
            if (file.Exists)
                file.Delete();
        }

        private string GetCacheForKey(string key) {
            // Since we're on Windows...
            var fileInfo = new FileInfo(key);
            var normalizedPath = fileInfo.FullName.ToUpperInvariant();

            using (var sha1 = SHA1.Create()) {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(normalizedPath));
                var fileName = new SoapHexBinary(hash).ToString();
                return Path.Combine(_cachePath, fileName);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            /* Do Nothing */
        }
    }
}
