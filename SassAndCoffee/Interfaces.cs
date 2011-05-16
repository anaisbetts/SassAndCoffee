using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SassAndCoffee
{
    public interface ISimpleFileCompiler
    {
        string[] InputFileExtensions { get; }
        string OutputFileExtension { get; }

        void Init(HttpApplication context);
        string ProcessFileContent(string inputFileContent);
    }
}
