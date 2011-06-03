using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using IronRuby;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace SassAndCoffee
{
    class SassModule
    {
        public dynamic Engine { get; set; }
        public dynamic SassOption { get; set; }
        public dynamic ScssOption { get; set; }
    }

		/// <summary>
		/// Defaults are provided for all of these, you do not need to specify
		/// them, they are used for compass integration.
		/// </summary>
		public static class SassConfiguration
		{
			/// <summary>
			/// The Ruby Load path, will be passed as SearchPaths to the IronRubyInterpreter
			/// </summary>
			public static String[] RubyLoadPaths { get; set; }

			/// <summary>
			/// Sass load paths, will be passed to Sass engine, this is where it will look
			/// for imported stylesheets
			/// </summary>
			public static string[] SassLoadPaths { get; set; }

			/// <summary>
			/// Used to provide a script that sets up your ruby environment. Compass and plain 
			/// Sass have different requirements.
			/// </summary>
			public static Func<ScriptEngine,ScriptSource> RubyStartupScript { get; set; }


			static SassConfiguration()
			{
				RubyLoadPaths = new[] {@"R:\lib\ironruby", @"R:\lib\ruby\1.9.1"};
				RubyStartupScript =
					engine =>
					engine.CreateScriptSourceFromString(Utility.ResourceAsString("SassAndCoffee.lib.sass_in_one.rb"), SourceCodeKind.File);
				SassLoadPaths = new[] {"."};
			}
		}
    public class SassFileCompiler : ISimpleFileCompiler
    {
        static ThreadLocal<SassModule> _sassModule;

        static SassFileCompiler()
        {
            _sassModule = new ThreadLocal<SassModule>(() => {
								var languageSetup = IronRuby.Ruby.CreateRubySetup();
            		languageSetup.Options.Add("SearchPaths", SassConfiguration.RubyLoadPaths);

                var srs = new ScriptRuntimeSetup() {HostType = typeof (ResourceAwareScriptHost)};
								srs.LanguageSetups.Add(languageSetup);

                var runtime = Ruby.CreateRuntime(srs);
                var engine = runtime.GetRubyEngine();

								var source = SassConfiguration.RubyStartupScript(engine);
                var scope = engine.CreateScope();
                source.Execute(scope);
    
                return new SassModule() {
                    Engine = scope.Engine.Runtime.Globals.GetVariable("Sass"),
                    SassOption = engine.Execute("{:syntax => :sass, :load_paths => " + GetSassLoadPaths(SassConfiguration.SassLoadPaths) + "}"),
                    ScssOption = engine.Execute("{:syntax => :scss, :load_paths => " + GetSassLoadPaths(SassConfiguration.SassLoadPaths) + "}"),
                };
            });
        }

    	public static string GetSassLoadPaths(params string[] sassLoadPaths) {
    		var forwardSlashedAndQuotedPaths =
    			sassLoadPaths.Select(x => String.Format("'{0}'", x.Replace("\\", "/")));

    		return "[" + String.Join(",", forwardSlashedAndQuotedPaths).TrimEnd(',') + "]";
    	}

    	public string[] InputFileExtensions {
            get { return new[] {".scss", ".sass"}; }
        }

        public string OutputFileExtension {
            get { return ".css"; }
        }

        public string OutputMimeType {
            get { return "text/css"; }
        }

        public void Init(HttpApplication context)
        {
        }

        public string ProcessFileContent(string inputFileContent)
        {
            dynamic opt = (inputFileContent.ToLowerInvariant().EndsWith("scss") ? _sassModule.Value.ScssOption : _sassModule.Value.SassOption);
            return (string) _sassModule.Value.Engine.compile(File.ReadAllText(inputFileContent), opt);
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
