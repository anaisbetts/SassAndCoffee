namespace SassAndCoffee.Ruby.Sass {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using SassAndCoffee.Core;

    public class SassCompilerContentTransform : ContentTransformBase {
        public const string MimeType = "text/css";
        public const string CompressedStateKey = "Sass_Compressed";
        public const int MaxCompileAttempts = 5;

        private Pool<ISassCompiler, SassCompilerProxy> _compilerPool =
            new Pool<ISassCompiler, SassCompilerProxy>(CreateAndInitializeSassCompiler);

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
            for (int i = 0; i < MaxCompileAttempts; ++i) {
                using (var compiler = _compilerPool.GetInstance()) {
                    try {
                        result = compiler.Compile(fileSource, state.Items.ContainsKey(CompressedStateKey), accessedFiles);
                        break;
                    } catch (SassSyntaxException) {
                        // We want these to bubble up so users can fix them.
                        throw;
                    } catch {
                        /* This really sucks, but the IronRuby engine fails often in multiple ways. There's nothing more specific
                         * we can catch here. On last try send it up for debugging - can't work around it. Do it this way (rather
                         * than tracking last exception and throwing outside loop) to preserve context.
                         */
                        if (i == MaxCompileAttempts - 1)
                            throw;
                    }
                }
            }

            if (result != null) {
                state.ReplaceContent(new ContentResult() {
                    Content = result,
                    MimeType = MimeType,
                    CacheInvalidationFileList = accessedFiles.ToArray(),
                });
            }
        }

        private static object _compilerInitializationLock = new object();
        private static ISassCompiler CreateAndInitializeSassCompiler() {
            var compiler = new SassCompiler();
            bool initialized = false;
            lock (_compilerInitializationLock) {
                while (!initialized) {
                    try {
                        compiler.Initialize();
                        initialized = true;
                    } catch { }
                }
            }
            return compiler;
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

            fileName = fileRoot + ".css";
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
