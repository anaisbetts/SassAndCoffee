namespace SassAndCoffee.Core
{
    using System.IO;

    public class CoffeeScriptCompiler : JavascriptBasedCompiler
    {
        public CoffeeScriptCompiler() : base("SassAndCoffee.lib.coffee-script.js", "compilify_cs") { }
    }

    public class CoffeeScriptFileCompiler : ISimpleFileCompiler
    {
        CoffeeScriptCompiler _engine;

        public string[] InputFileExtensions {
            get { return new[] { ".coffee" }; }
        }

        public string OutputFileExtension {
            get { return ".js"; }
        }

        public string OutputMimeType {
            get { return "text/javascript"; }
        }

        public CoffeeScriptFileCompiler(CoffeeScriptCompiler engine = null)
        {
            _engine = engine;
        }

        public void Init()
        {
            _engine = _engine ?? new CoffeeScriptCompiler();
        }

        public string ProcessFileContent(string inputFileContent)
        {
            return _engine.Compile(File.ReadAllText(inputFileContent));
        }

        public string GetFileChangeToken(string inputFileContent)
        {
            return "";
        }
    }
}