namespace SassAndCoffee.Core.Compilers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    public class FileConcatenationCompiler : ISimpleFileCompiler
    {
        private ICompilerHost _host;

        private IContentCompiler _compiler;

        public string[] InputFileExtensions
        {
            get { return new[] { ".combine" }; }
        }

        public string OutputFileExtension
        {
            get { return ".js"; }
        }

        public string OutputMimeType
        {
            get { return "text/javascript"; }
        }

        public void Init(ICompilerHost host)
        {
            this._host = host;
        }

        static readonly Regex _commentRegex = new Regex("#.*$", RegexOptions.Compiled);

        public FileConcatenationCompiler(IContentCompiler compiler)
        {
            _compiler = compiler;
        }

        public string ProcessFileContent(string inputFileContent)
        {
            var combineFileNames = this.GetCombineFileNames(inputFileContent);

            var allText = combineFileNames.Select(
                x => !this._compiler.CanCompile(x) 
                        ? String.Empty
                        : this._compiler.GetCompiledContent(x).Contents).ToArray();

            return allText.Aggregate(new StringBuilder(), (acc, x) =>
            {
                acc.Append(x);
                acc.Append("\n");
                return acc;
            }).ToString();
        }

        public string GetFileChangeToken(string inputFileContent)
        {
            return "";

            // TODO - fixup - may require a change to the compiler interface

            //var md5sum = MD5.Create();
            //var ms = this.GetCombineFileNames(inputFileContent)
            //    .Select(x =>
            //    {
            //        var compiler = _host.MapPathToCompiler(x);
            //        return (compiler != null ? compiler.FindInputFileGivenOutput(x) ?? "" : "");
            //    })
            //    .Select(x => new FileInfo(x))
            //    .Where(x => x.Exists)
            //    .Select(x => x.LastWriteTimeUtc.Ticks)
            //    .Aggregate(new MemoryStream(), (acc, x) =>
            //    {
            //        var buf = BitConverter.GetBytes(x);
            //        acc.Write(buf, 0, buf.Length);
            //        return acc;
            //    });

            //return md5sum.ComputeHash(ms.GetBuffer()).Aggregate(new StringBuilder(), (acc, x) =>
            //{
            //    acc.Append(x.ToString("x"));
            //    return acc;
            //}).ToString();
        }

        IEnumerable<string> GetCombineFileNames(string inputFileContent)
        {
            return File.ReadAllLines(inputFileContent)
                .Select(x => _commentRegex.Replace(x, String.Empty))
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .Where(x => !x.ToLowerInvariant().EndsWith(".combine"))
                .ToArray();
        }
    }
}