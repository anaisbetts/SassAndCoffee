﻿namespace SassAndCoffee.Ruby.Sass {
    using System.Collections.Generic;
    using System.IO;
    using SassAndCoffee.Core;

    public class SassCompilerContentTransform : ContentTransformBase {
        public const string MimeType = "text/css";

        private Pool<ISassCompiler, SassCompilerProxy> _compilerPool =
            new Pool<ISassCompiler, SassCompilerProxy>(() => new SassCompiler());

        public override void Execute(ContentTransformState state) {
            // Support 404, not just 500
            if (state.RootPath == null)
                return;

            var fileSource = FindFileFromRoot(state.RootPath);
            if (fileSource == null) return;

            string result = null;
            var accessedFiles = new List<string>();
            using (var compiler = _compilerPool.GetInstance()) {
                result = compiler.Compile(fileSource, accessedFiles);
            }

            if (result != null) {
                state.ReplaceContent(new ContentResult() {
                    Content = result,
                    MimeType = MimeType,
                    CacheInvalidationFileList = accessedFiles,
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
