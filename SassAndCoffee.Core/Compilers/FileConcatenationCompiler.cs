namespace SassAndCoffee.Core.Compilers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    using SassAndCoffee.Core.Extensions;

    public class FileConcatenationCompiler : ISimpleFileCompiler
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

        public void Init(ICompilerHost host)
        {
        }

        static Regex _commentRegex;
        public string ProcessFileContent(string inputFileContent)
        {
            var absolutePaths = parseCombineFileToPaths(inputFileContent);

            var allText = absolutePaths./*AsParallel().*/Select(x => {
                string inputFile = null;

                var compiler = _host.MapPathToCompiler(x);
                if (compiler == null || (inputFile = compiler.FindInputFileGivenOutput(x)) == null) {
                    throw new Exception(String.Format("Compiler not found for file: '{0}'", x));
                }

                return compiler.ProcessFileContent(inputFile);
            }).ToArray();
            
            return allText.Aggregate(new StringBuilder(), (acc, x) => {
                acc.Append(x);
                acc.Append("\n");
                return acc;
            }).ToString();
        }

        public string GetFileChangeToken(string inputFileContent)
        {
            var md5sum = MD5.Create();
            var ms = parseCombineFileToPaths(inputFileContent)
                .Select(x => {
                    var compiler = _host.MapPathToCompiler(x);
                    return (compiler != null ? compiler.FindInputFileGivenOutput(x) ?? "" : "");
                })
                .Select(x => new FileInfo(x))
                .Where(x => x.Exists)
                .Select(x => x.LastWriteTimeUtc.Ticks)
                .Aggregate(new MemoryStream(), (acc, x) => {
                    var buf = BitConverter.GetBytes(x);
                    acc.Write(buf, 0, buf.Length);
                    return acc;
                });

            return md5sum.ComputeHash(ms.GetBuffer()).Aggregate(new StringBuilder(), (acc, x) => {
                acc.Append(x.ToString("x"));
                return acc;
            }).ToString();
        }

        string[] parseCombineFileToPaths(string inputFileContent)
        {
            return new string[] { };
            //if (_commentRegex == null) {
            //    var re = new Regex("#.*$");
            //    _commentRegex = re;
            //}

            //return File.ReadAllLines(inputFileContent)
            //    .Select(x => _commentRegex.Replace(x, String.Empty))
            //    .Where(x => !String.IsNullOrWhiteSpace(x))
            //    .Where(x => !x.ToLowerInvariant().EndsWith(".combine"))
            //    .Select(x => relativeToAbsolutePath(x, this._app))
            //    .ToArray();
        }

        public string relativeToAbsolutePath(string relativePath)
        {
            return string.Empty;
            // TODO - Fixup
            //if (relativePath[0] == '~') {
            //    return context.Request.MapPath(relativePath);
            //}

            //string baseDir = Path.GetDirectoryName(context.Request.PhysicalPath);

            //var fi = new FileInfo(Path.Combine(baseDir, relativePath));
            //if (!fi.FullName.ToLowerInvariant().StartsWith(baseDir.ToLowerInvariant())) {
            //    return String.Empty;
            //}
            //return fi.FullName;
        }
    }
}