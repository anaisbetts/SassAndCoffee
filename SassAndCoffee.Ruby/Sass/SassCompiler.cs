namespace SassAndCoffee.Ruby.Sass {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using IronRuby;
    using Microsoft.Scripting;
    using Microsoft.Scripting.Hosting;
    using SassAndCoffee.Core;

    public class SassCompiler : ISassCompiler {
        private ScriptEngine _engine;
        private ScriptScope _scope;
        private ResourceRedirectionPlatformAdaptationLayer _pal;
        private dynamic _sassCompiler;
        private dynamic _sassOption;
        private dynamic _scssOption;
        private dynamic _sassOptionCompressed;
        private dynamic _scssOptionCompressed;
        private bool _initialized = false;
        private object _lock = new object();

        public string Compile(string path, bool compressed, IList<string> dependentFileList) {
            if (path == null)
                throw new ArgumentException("source cannot be null.", "source");

            var pathInfo = new FileInfo(path);
            if (!pathInfo.Exists)
                return null;

            lock (_lock) {
                Initialize();

                dynamic compilerOptions;
                if (pathInfo.Extension.Equals(".sass", StringComparison.OrdinalIgnoreCase)) {
                    compilerOptions = compressed ? _sassOptionCompressed : _sassOption;
                } else {
                    compilerOptions = compressed ? _scssOptionCompressed : _scssOption;
                }

                var directoryPath = pathInfo.DirectoryName;
                if (!directoryPath.Contains("\'")) {
                    var statement = String.Format("Dir.chdir '{0}'", directoryPath);
                    _engine.Execute(statement, _scope);
                }

                if (dependentFileList != null) {
                    dependentFileList.Add(pathInfo.FullName);
                    _pal.OnOpenInputFileStream = (accessedFile) => {
                        if (!accessedFile.Contains(".sass-cache"))
                            dependentFileList.Add(accessedFile);
                    };
                }

                string result;
                try {
                    result = (string)_sassCompiler.compile(File.ReadAllText(pathInfo.FullName), compilerOptions);
                } catch (Exception e) {
                    // Provide more information for SassSyntaxErrors
                    if (e.Message == "Sass::SyntaxError") {
                        var messageFromException = buildErrorMessageFromException(pathInfo, e);
                        throw new Exception(messageFromException, e);
                    }
                    else {
                        throw;
                    }
                } finally {
                    _pal.OnOpenInputFileStream = null;
                }
                return result;
            }
        }

        public string CompileScss(string input, bool compressed) {
            lock (_lock) {
                Initialize();

                string result;
                try {
                    result = (string) _sassCompiler.compile(input, compressed ? _scssOptionCompressed : _scssOption);
                }
                catch (Exception e) {
                    // Provide more information for SassSyntaxErrors
                    if (e.Message == "Sass::SyntaxError") {
                        var messageFromException = buildErrorMessageFromException(null, e);
                        throw new Exception(messageFromException, e);
                    }

                    throw;
                }
                return result;
            }
        }

        public string CompileSass(string input, bool compressed) {
            lock (_lock) {
                Initialize();

                string result;
                try {
                    result = (string) _sassCompiler.compile(input, compressed ? _sassOptionCompressed : _sassOption);
                }
                catch (Exception e) {
                    // Provide more information for SassSyntaxErrors
                    if (e.Message == "Sass::SyntaxError") {
                        var messageFromException = buildErrorMessageFromException(null, e);
                        throw new Exception(messageFromException, e);
                    }

                    throw;
                }
                return result;
            }
        }

        private static string buildErrorMessageFromException(FileSystemInfo pathInfo, dynamic error) {
            var fullName = pathInfo != null ? pathInfo.FullName : "No file";
            var sb = new StringBuilder();
            sb.AppendFormat("{0}\n\n", error.to_s());
            sb.AppendFormat("Backtrace:\n{0}\n\n", error.sass_backtrace_str(fullName) ?? "");
            sb.AppendFormat("FileName: {0}\n\n", error.sass_filename() ?? fullName);
            sb.AppendFormat("MixIn: {0}\n\n", error.sass_mixin() ?? "");
            sb.AppendFormat("Line Number: {0}\n\n", error.sass_line() ?? "");
            sb.AppendFormat("Sass Template:\n{0}\n\n", error.sass_template ?? "");
            return sb.ToString();
        }

        private void Initialize() {
            if (!_initialized) {
                _pal = new ResourceRedirectionPlatformAdaptationLayer();
                var srs = new ScriptRuntimeSetup() {
                    HostType = typeof(SassCompilerScriptHost),
                    HostArguments = new List<object>() { _pal },
                };
                srs.AddRubySetup();
                var runtime = Ruby.CreateRuntime(srs);
                _engine = runtime.GetRubyEngine();

                // NB: 'R:\{345ED29D-C275-4C64-8372-65B06E54F5A7}' is a garbage path that the PAL override will 
                // detect and attempt to find via an embedded Resource file
                _engine.SetSearchPaths(new List<string>() { 
                    @"R:\{345ED29D-C275-4C64-8372-65B06E54F5A7}\lib\ironruby",
                    @"R:\{345ED29D-C275-4C64-8372-65B06E54F5A7}\lib\ruby\1.9.1" });

                var source = _engine.CreateScriptSourceFromString(
                    Utility.ResourceAsString("lib.sass_in_one.rb", typeof(SassCompiler)),
                    SourceCodeKind.File);
                _scope = _engine.CreateScope();
                source.Execute(_scope);

                _sassCompiler = _scope.Engine.Runtime.Globals.GetVariable("Sass");
                _sassOption = _engine.Execute("{:cache => false, :syntax => :sass}");
                _scssOption = _engine.Execute("{:cache => false, :syntax => :scss}");
                _sassOptionCompressed = _engine.Execute("{:cache => false, :syntax => :sass, :style => :compressed}");
                _scssOptionCompressed = _engine.Execute("{:cache => false, :syntax => :scss, :style => :compressed}");

                _initialized = true;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
        }
    }
}
