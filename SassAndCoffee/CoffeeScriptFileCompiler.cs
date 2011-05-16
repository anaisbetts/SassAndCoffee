using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace SassAndCoffee
{
    public class CoffeeScriptFileCompiler : ISimpleFileCompiler
    {
        CoffeeScriptCompiler _engine;

        public string[] InputFileExtensions
        {
            get { return new[] { ".coffee" }; }
        }

        public string OutputFileExtension
        {
            get { return ".js"; }
        }

        public void Init(HttpApplication context)
        {
            _engine = new CoffeeScriptCompiler();
        }

        public string ProcessFileContent(string inputFileContent)
        {
            try
            {
                return _engine.Compile(File.ReadAllText(inputFileContent));
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
