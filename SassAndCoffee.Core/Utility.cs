namespace SassAndCoffee.Core {
    using System.IO;
    using System.Reflection;
    using System.Text;

    public class Utility {
        public static string ResourceAsString(string resource) {
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            using (var reader = new StreamReader(resourceStream))
                return reader.ReadToEnd();
        }
    }
}
