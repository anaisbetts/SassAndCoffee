namespace SassAndCoffee.Ruby.Sass {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using IronRuby;
    using IronRuby.Runtime;
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

        public string Compile(string path, bool compressed, IList<string> dependentFileList) {
            if (path == null)
                throw new ArgumentException("source cannot be null.", "source");

            if (!_initialized)
                throw new InvalidOperationException("Compiler must be initialized first.");

            var pathInfo = new FileInfo(path);
            if (!pathInfo.Exists)
                return null;

            string result;
            try {
                dynamic compilerOptions;
                if (pathInfo.Extension.Equals(".sass", StringComparison.OrdinalIgnoreCase)) {
                    compilerOptions = compressed ? _sassOptionCompressed : _sassOption;
                } else /* .scss and .css */ {
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
                        lock (dependentFileList) {
                            dependentFileList.Add(accessedFile);
                        }
                    };
                }

                using (var stream = File.Open(pathInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(stream)) {
                    var contents = reader.ReadToEnd();
                    result = (string)_sassCompiler.compile(contents, compilerOptions);
                }
            } catch (Exception e) {
                // Provide more information for SassSyntaxErrors
                if (SassSyntaxException.IsSassSyntaxError(e)) {
                    throw SassSyntaxException.FromSassSyntaxError(e, pathInfo.FullName);
                }

                var rubyEx = RubyExceptionData.GetInstance(e);
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}\n\n", rubyEx.Message)
                  .AppendFormat("Backtrace:\n");
                foreach (var frame in rubyEx.Backtrace) {
                    sb.AppendFormat("  {0}\n", frame);
                }
                throw new Exception(sb.ToString(), e);
            } finally {
                _pal.OnOpenInputFileStream = null;
            }
            return result;
        }

        public void Initialize() {
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
                    SourceCodeKind.Statements).Compile();
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
    }
}
