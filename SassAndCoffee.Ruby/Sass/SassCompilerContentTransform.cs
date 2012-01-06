namespace SassAndCoffee.Ruby.Sass {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using SassAndCoffee.Core;

    public class SassCompilerContentTransform : ContentTransformBase {
        public const string MimeType = "text/css";
        public const string CompressedStateKey = "Sass_Compressed";

        private Pool<ISassCompiler, SassCompilerProxy> _compilerPool =
            new Pool<ISassCompiler, SassCompilerProxy>(() => new SassCompiler());

        public override void PreExecute(ContentTransformState state) {
            if (state.Path.EndsWith(".min.css", StringComparison.OrdinalIgnoreCase)) {
                state.Items.Add(CompressedStateKey, true);
                var newPath = state.Path
                    .ToLowerInvariant()
                    .Replace(".min.css", ".css");
                state.RemapPath(newPath);
            }
            base.PreExecute(state);
        }

        public override void Execute(ContentTransformState state) {
            // Support 404, not just 500
            if (state.RootPath == null)
                return;

            var fileSource = FindFileFromRoot(state.RootPath);
            if (fileSource == null) return;

            string result = null;
            var accessedFiles = new List<string>();
            using (var compiler = _compilerPool.GetInstance()) {
                result = compiler.Compile(fileSource, state.Items.ContainsKey(CompressedStateKey), accessedFiles);
            }

            if (result != null) {
                state.ReplaceContent(new ContentResult() {
                    Content = result,
                    MimeType = MimeType,
                    CacheInvalidationFileList = accessedFiles.ToArray(),
                });
            }
        }

        private string FindFileFromRoot(string fileRoot) {
            if (fileRoot == null)
                return null;

            var fileName = fileRoot + ".scss";
            if (File.Exists(fileName))
                return fileName;

            fileName = fileRoot + ".sass";
            if (File.Exists(fileName))
                return fileName;

            return null;
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (_compilerPool != null) {
                    _compilerPool.Dispose();
                    _compilerPool = null;
                }
            }
        }
    }
}
