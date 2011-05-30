using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SassAndCoffee
{
    public class ConcatenationFileHandler : ISimpleFileCompiler
    {
        public string[] InputFileExtensions {
            get { return new[] {".combine"}; }
        }

        public string OutputFileExtension {
            get { return ".js"; }
        }

        public string OutputMimeType {
            get { return "text/javascript"; }
        }

        ICompilerHost _host;
        HttpApplication _app;

        public ConcatenationFileHandler(ICompilerHost host)
        {
            _host = host;
        }

        public void Init(HttpApplication context)
        {
            _app = context;
        }

        static Regex _commentRegex;
        public string ProcessFileContent(string inputFileContent)
        {
            if (_commentRegex == null) {
                var re = new Regex("#.*$");
                _commentRegex = re;
            }

            try {
                var absolutePaths = File.ReadAllLines(inputFileContent)
                    .Select(x => _commentRegex.Replace(x, String.Empty))
                    .Where(x => !String.IsNullOrWhiteSpace(x))
                    .Where(x => !x.ToLowerInvariant().EndsWith(".combine"))
                    .Select(x => relativeToAbsolutePath(x, _app))
                    .ToArray();

                return absolutePaths.Aggregate(new StringBuilder(), (acc, x) => {
                    string inputFile = null;

                    var compiler = _host.MapPathToCompiler(x);
                    if (compiler == null || (inputFile = compiler.FindInputFileGivenOutput(x)) == null) {
                        throw new Exception(String.Format("Compiler not found for file: '{0}'", x));
                    }

                    acc.Append(compiler.ProcessFileContent(inputFile));
                    acc.Append("\n");
                    return acc;
                }).ToString();
            } catch (Exception ex) {
                return ex.Message;
            }
        }

        public string relativeToAbsolutePath(string relativePath, HttpApplication context)
        {
            if (relativePath[0] == '~') {
                return context.Request.MapPath(relativePath);
            }

            string baseDir = Path.GetDirectoryName(context.Request.PhysicalPath);

            var fi = new FileInfo(Path.Combine(baseDir, relativePath));
            if (!fi.FullName.ToLowerInvariant().StartsWith(baseDir.ToLowerInvariant())) {
                return String.Empty;
            }
            return fi.FullName;
        }
    }
}