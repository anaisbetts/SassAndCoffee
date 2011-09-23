using System.Diagnostics;
using System.Linq.Expressions;

using SassAndCoffee.Core.Extensions;

namespace SassAndCoffee.Core.Compilers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

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
            public VirtualFilePAL PlatformAdaptationLayer { get; set; }
        }

        static TrashStack<SassModule> _sassModule;
        internal static string RootAppPath;

        static SassFileCompiler()
        {
            _sassModule = new TrashStack<SassModule>(() => {
                var srs = new ScriptRuntimeSetup() {
                    HostType = typeof (ResourceAwareScriptHost),
                    //DebugMode = Debugger.IsAttached
                };
                srs.AddRubySetup();
                var runtime = Ruby.CreateRuntime(srs);
                var engine = runtime.GetRubyEngine();

                // NB: 'R:\' is a garbage path that the PAL override below will 
                // detect and attempt to find via an embedded Resource file
                engine.SetSearchPaths(new[] {@"R:/lib/ironruby", @"R:/lib/ruby/1.9.1"});

                var source = engine.CreateScriptSourceFromString(Utility.ResourceAsString("SassAndCoffee.Core.lib.sass_in_one.rb"), "R:/lib/sass_in_one.rb", SourceCodeKind.File);
                var scope = engine.CreateScope();
                source.Execute(scope);
                return new SassModule {
                    PlatformAdaptationLayer = (VirtualFilePAL)runtime.Host.PlatformAdaptationLayer,
                    Engine = scope.Engine.Runtime.Globals.GetVariable("Sass"),
                    SassOption = engine.Execute(@"{:syntax => :sass, :cache_location => ""C:/""}"),
                    ScssOption = engine.Execute(@"{:syntax => :scss, :cache_location => ""C:/""}"),
                    ExecuteRubyCode = code => engine.Execute(code, scope)
                };
            });
        }

        public IEnumerable<string> InputFileExtensions {
            get {
                yield return ".scss";
                yield return ".sass";
            }
        }

        public string OutputFileExtension {
            get { return ".css"; }
        }

        public string OutputMimeType {
            get { return "text/css"; }
        }

        public string ProcessFileContent(ICompilerFile inputFileContent)
        {
            using (var sassModule = _sassModule.Get()) {
                dynamic opt = (inputFileContent.Name.EndsWith(".scss", StringComparison.OrdinalIgnoreCase) ? sassModule.Value.ScssOption : sassModule.Value.SassOption);
                using (sassModule.Value.PlatformAdaptationLayer.SetCompilerFile(inputFileContent)) {
                    return (string)sassModule.Value.Engine.compile(inputFileContent.ReadAllText(), opt);
                }
            }
        }

        public string GetFileChangeToken(ICompilerFile inputFileContent)
        {
            return "";
        }
    }

    public class ResourceAwareScriptHost : ScriptHost
    {
        private readonly PlatformAdaptationLayer _innerPal = new VirtualFilePAL();

        public override PlatformAdaptationLayer PlatformAdaptationLayer {
            get {
                return _innerPal;
            }
        }
    }
}
