namespace SassAndCoffee.Core.Pipeline {
    using System.IO;

    public class JavaScriptCombineContentTransform : ContentTransformBase {
        public override void Execute(ContentTransformState state) {
            // We're a content provider.  If content is already set, do nothing.
            if (state.Content != null)
                return;

            // Support 404, not just 500
            if (state.RootPath == null)
                return;

            var fileInfo = new FileInfo(state.RootPath + ".combine");
            if (fileInfo.Exists) {
                state.AddCacheInvalidationFiles(new string[] { fileInfo.FullName });

                var lines = File.ReadLines(fileInfo.FullName);
                foreach (var line in lines) {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                        continue;
                    var newPath = Path.Combine(fileInfo.DirectoryName, trimmed);
                    var newContent = state.Pipeline.ProcessRequest(newPath);
                    if (newContent != null) {
                        state.AppendContent(newContent);
                    }
                }
            }

        }
    }
}
