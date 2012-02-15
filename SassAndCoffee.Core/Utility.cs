namespace SassAndCoffee.Core {
    using System;
    using System.IO;

    public static class Utility {
        public static string ResourceAsString(string resource, Type scope) {
            if (scope == null)
                throw new ArgumentNullException("scope");

            var resourceStream = scope.Assembly.GetManifestResourceStream(scope, resource);
            using (var reader = new StreamReader(resourceStream))
                return reader.ReadToEnd();
        }
    }
}
