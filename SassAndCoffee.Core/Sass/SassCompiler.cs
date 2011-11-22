namespace SassAndCoffee.Core.Sass {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using IronRuby;
    using Microsoft.Scripting;
    using Microsoft.Scripting.Hosting;

    public class SassCompiler : ISassCompiler {
        private ScriptEngine _engine;
        private ScriptScope _scope;
        private dynamic _sassCompiler;
        private dynamic _sassOption;
        private dynamic _scssOption;
        private bool _initialized = false;
        private object _lock = new object();

        public string Compile(string path) {
            if (path == null)
                throw new ArgumentException("source cannot be null.", "source");

            var pathInfo = new FileInfo(path);
            if (!pathInfo.Exists)
                return null;

            lock (_lock) {
                Initialize();

                dynamic compilerOptions;
                if (pathInfo.Extension.Equals(".sass", StringComparison.OrdinalIgnoreCase))
                    compilerOptions = _sassOption;
                else
                    compilerOptions = _scssOption;

                var directoryPath = pathInfo.DirectoryName;
                if (!directoryPath.Contains("\'")) {
                    var statement = String.Format("Dir.chdir '{0}'", directoryPath);
                    _engine.Execute(statement, _scope);
                }

                return (string)_sassCompiler.compile(File.ReadAllText(pathInfo.FullName), compilerOptions);
            }
        }

        private void Initialize() {
            if (!_initialized) {
                var srs = new ScriptRuntimeSetup() { 
                    HostType = typeof(SassCompilerScriptHost),
                    HostArguments = new List<object>() { new ResourceRedirectionPlatformAdaptationLayer() },
                };
                srs.AddRubySetup();
                var runtime = Ruby.CreateRuntime(srs);
                _engine = runtime.GetRubyEngine();

                // NB: 'R:\' is a garbage path that the PAL override below will 
                // detect and attempt to find via an embedded Resource file
                _engine.SetSearchPaths(new List<string>() { @"R:\lib\ironruby", @"R:\lib\ruby\1.9.1" });

                var source = _engine.CreateScriptSourceFromString(
                    Utility.ResourceAsString("lib.sass_in_one.rb", typeof(SassCompiler)),
                    SourceCodeKind.File);
                _scope = _engine.CreateScope();
                source.Execute(_scope);

                _sassCompiler = _scope.Engine.Runtime.Globals.GetVariable("Sass");
                _sassOption = _engine.Execute("{:syntax => :sass}");
                _scssOption = _engine.Execute("{:syntax => :scss}");

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
