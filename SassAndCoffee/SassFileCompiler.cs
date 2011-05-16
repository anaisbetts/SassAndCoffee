using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using IronRuby;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace SassAndCoffee
{
    public class SassFileCompiler : ISimpleFileCompiler
    {
        dynamic _sassModule;
        dynamic _scssOption, _sassOption;

        public string[] InputFileExtensions {
            get { return new[] {".scss", ".sass"}; }
        }

        public string OutputFileExtension {
            get { return ".sass"; }
        }

        public void Init(HttpApplication context)
        {
            var srs = new ScriptRuntimeSetup() {HostType = typeof (ResourceAwareScriptHost)};
            srs.AddRubySetup();
            var runtime = Ruby.CreateRuntime(srs);
            var engine = runtime.GetRubyEngine();
            engine.SetSearchPaths(new List<string>() {@"R:\lib\ironruby", @"R:\lib\ruby\1.9.1"});

            var source = engine.CreateScriptSourceFromString(Utility.ResourceAsString("SassAndCoffee.lib.sass_in_one.rb"), SourceCodeKind.File);
            var scope = engine.CreateScope();
            source.Execute(scope);

            _scssOption = engine.Execute("{:syntax => :scss}"); 
            _sassOption = engine.Execute("{:syntax => :sass}"); 
            _sassModule = scope.Engine.Runtime.Globals.GetVariable("Sass");
        }

        public string ProcessFileContent(string inputFileContent)
        {
            dynamic opt = (inputFileContent.ToLowerInvariant().EndsWith("scss") ? _scssOption : _sassOption);
            try {
                return (string) _sassModule.compile(File.ReadAllText(inputFileContent), opt);
            } catch (Exception ex) {
                return ex.Message;
            }
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
        public override System.IO.Stream OpenInputFileStream(string path)
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

            return base.FileExists(path);
        }

        string pathToResourceName(string path)
        {
            var ret = path
                .Replace("1.9.1", "_1._9._1")
                .Replace('\\', '.')
                .Replace('/', '.')
                .Replace("R:", "SassAndCoffee");
            return ret;
        }
    }
}
