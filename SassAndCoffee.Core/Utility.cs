namespace SassAndCoffee.Core {
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography;

    public static class Utility {
        public static string ResourceAsString(string resource, Type scope = null) {
            var assembly = Assembly.GetExecutingAssembly();
            Stream resourceStream;
            
            if(scope != null)
                resourceStream = assembly.GetManifestResourceStream(scope, resource);
            else
                resourceStream = assembly.GetManifestResourceStream(resource);

            using (resourceStream)
            using (var reader = new StreamReader(resourceStream))
                return reader.ReadToEnd();
        }

        public static string CreateETag(byte[] source) {
            using (var hash = SHA1.Create()) {
                hash.Initialize();
                return string.Format("\"{0}\"", Convert.ToBase64String(hash.ComputeHash(source)));
            }
        }
    }
}
