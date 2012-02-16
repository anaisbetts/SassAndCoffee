namespace SassAndCoffee.Core {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    [Serializable]
    public class CachedContentResult : ContentResult {
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Simplify serialization.")]
        public byte[][] Hashes;

        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Simplify serialization.")]
        public bool HashesInitialized;

        public void ComputeHashes() {
            var numFiles = CacheInvalidationFileList.Length;
            Hashes = new byte[numFiles][];
            for (int i = 0; i < numFiles; ++i) {
                Hashes[i] = ComputeHash(CacheInvalidationFileList[i]);
            }
            HashesInitialized = true;
        }

        public bool ValidateHashes() {
            if (!HashesInitialized)
                return false;

            if (CacheInvalidationFileList.Length != Hashes.Length)
                return false;

            var numFiles = CacheInvalidationFileList.Length;
            for (int i = 0; i < numFiles; ++i) {
                var currentHash = ComputeHash(CacheInvalidationFileList[i]);
                if (!currentHash.SequenceEqual(Hashes[i]))
                    return false;
            }

            return true;
        }

        public static CachedContentResult FromContentResult(ContentResult contentResult) {
            if (contentResult == null)
                throw new ArgumentNullException("contentResult");

            return new CachedContentResult {
                CacheInvalidationFileList = contentResult.CacheInvalidationFileList,
                Content = contentResult.Content,
                MimeType = contentResult.MimeType,
            };
        }

        private static byte[] ComputeHash(string fileName) {
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
            using (var sha1 = SHA1.Create()) {
                return sha1.ComputeHash(fileStream);
            }
        }
    }
}
