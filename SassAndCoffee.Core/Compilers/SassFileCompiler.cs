namespace SassAndCoffee.Core.Compilers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using IronRuby;

    using Microsoft.Scripting;
    using Microsoft.Scripting.Hosting;

    public class SassFileCompiler : ISimpleFileCompiler
    {
        private class SassModule
        {
            public dynamic Engine { get; set; }
            public dynamic SassOption { get; set; }
            public dynamic ScssOption { get; set; }
            public Action<string> ExecuteRubyCode { get; set; }
        }

        static TrashStack<SassModule> _sassModule;

        static SassFileCompiler()
        {
            _sassModule = new TrashStack<SassModule>(() => {
                var srs = new ScriptRuntimeSetup() {HostType = typeof (ResourceAwareScriptHost)};
                srs.AddRubySetup();
                var runtime = Ruby.CreateRuntime(srs);
                var engine = runtime.GetRubyEngine();

                // NB: 'R:\' is a garbage path that the PAL override below will 
                // detect and attempt to find via an embedded Resource file
                engine.SetSearchPaths(new List<string>() {@"R:\lib\ironruby", @"R:\lib\ruby\1.9.1"});
    
                var source = engine.CreateScriptSourceFromString(Utility.ResourceAsString("SassAndCoffee.Core.lib.sass_in_one.rb"), SourceCodeKind.File);
                var scope = engine.CreateScope();
                source.Execute(scope);

                return new SassModule() {
                    Engine = scope.Engine.Runtime.Globals.GetVariable("Sass"),
                    SassOption = engine.Execute("{:syntax => :sass}"),
                    ScssOption = engine.Execute("{:syntax => :scss}"),
                    ExecuteRubyCode = code => engine.Execute(code, scope),
                };
            });
        }

        public string[] InputFileExtensions {
            get { return new[] {".scss", ".sass"}; }
        }

        public string OutputFileExtension {
            get { return ".css"; }
        }

        public void Init(ICompilerHost host)
        {
        }

        public string ProcessFileContent(string inputFileContent)
        {
            using (var sassModule = _sassModule.Get()) {
                dynamic opt = (inputFileContent.ToLowerInvariant().EndsWith("scss") ? sassModule.Value.ScssOption : sassModule.Value.SassOption);

                if (!inputFileContent.Contains('\'')) {
                    sassModule.Value.ExecuteRubyCode(String.Format("Dir.chdir '{0}'", Path.GetDirectoryName(inputFileContent)));
                }
    
                return (string) sassModule.Value.Engine.compile(File.ReadAllText(inputFileContent), opt);
            }
        }

        public string GetFileChangeToken(string inputFileContent)
        {
            return "";
        }
    }

    public class ResourceAwareScriptHost : ScriptHost
    {
        PlatformAdaptationLayer _innerPal = null;
        public override PlatformAdaptationLayer PlatformAdaptationLayer {
            get {
                if (_innerPal == null) {
                    _innerPal = new ResourceAwarePAL();
                }
                return _innerPal;
            }
        }
    }

    public class ResourceAwarePAL : PlatformAdaptationLayer
    {
        public override Stream OpenInputFileStream(string path)
        {
            var ret = Assembly.GetExecutingAssembly().GetManifestResourceStream(pathToResourceName(path));
            if (ret != null) {
                return ret;
            }

            return base.OpenInputFileStream(path);
        }

        public override bool FileExists(string path)
        {
            if (Assembly.GetExecutingAssembly().GetManifestResourceInfo(pathToResourceName(path)) != null) {
                return true;
            }

            if (path.EndsWith("css")) {
                int a = 1;
            }

            return base.FileExists(path);
        }

        string pathToResourceName(string path)
        {
            var ret = path
                .Replace("1.9.1", "_1._9._1")
                .Replace('\\', '.')
                .Replace('/', '.')
                .Replace("R:", "SassAndCoffee.Core");
            return ret;
        }
    }
}
