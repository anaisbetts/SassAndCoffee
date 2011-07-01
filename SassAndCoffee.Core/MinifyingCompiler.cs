namespace SassAndCoffee.Core
{
    using System.IO;

    public class MinifyingCompiler : JavascriptBasedCompiler
    {
        public MinifyingCompiler() : base("SassAndCoffee.lib.uglify.js", "compilify_ujs") { }
    }

    public class MinifyingFileCompiler : ISimpleFileCompiler
    {
        TrashStack<CoffeeScriptCompiler> _coffeeEngine;
        TrashStack<MinifyingCompiler> _engine;

        public string[] InputFileExtensions {
            get { return new[] {".js", ".coffee"}; }
        }

        public string OutputFileExtension {
            get { return ".min.js"; }
        }

        public string OutputMimeType {
            get { return "text/javascript"; }
        }

        public MinifyingFileCompiler(CoffeeScriptCompiler coffeeScriptEngine)
        {
            _coffeeEngine = new TrashStack<CoffeeScriptCompiler>(() => new CoffeeScriptCompiler());
            _engine = new TrashStack<MinifyingCompiler>(() => new MinifyingCompiler());
        }

        public void Init()
        {
        }

        public string ProcessFileContent(string inputFileContent)
        {
            string text = File.ReadAllText(inputFileContent);

            if (inputFileContent.ToLowerInvariant().EndsWith(".coffee")) {
                using(var coffeeEngine = _coffeeEngine.Get()) {
                    text = coffeeEngine.Value.Compile(text);
                }
            }

            string ret;
            using (var engine = _engine.Get()) {
                ret = engine.Value.Compile(text);
            }

            return ret;
        }

        public string GetFileChangeToken(string inputFileContent)
        {
            return "";
        }
    }
}