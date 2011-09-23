using SassAndCoffee.Core.Extensions;

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
        private static readonly Regex _lineRegex = new Regex(@"(?<=^\s*)(?!=\#|\s)((?!\.combined\s*$).)+?(?=\s*$)", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.ExplicitCapture|RegexOptions.IgnoreCase|RegexOptions.Singleline);

        private readonly IContentCompiler _compiler;

        public IEnumerable<string> InputFileExtensions {
            get { yield return ".combine"; }
        }

        public string OutputFileExtension {
            get { return ".js"; }
        }

        public string OutputMimeType {
            get { return "text/javascript"; }
        }

        public FileConcatenationCompiler(IContentCompiler compiler) 
        {
            if (compiler == null) {
                throw new ArgumentNullException("compiler");
            }
            _compiler = compiler;
        }

        public string ProcessFileContent(ICompilerFile inputFileContent) 
        {
            IEnumerable<ICompilerFile> combineFileNames = GetCombineFileNames(inputFileContent);
            string[] allText = combineFileNames
                    .Select(x => _compiler.CanCompile(x)
                                         ? _compiler.GetCompiledContent(x).Contents
                                         : String.Empty)
                    .ToArray();
            return allText.Aggregate(new StringBuilder(), (acc, x) => {
                                                              acc.Append(x);
                                                              acc.Append("\n");
                                                              return acc;
                                                          }).ToString();
        }

        public string GetFileChangeToken(ICompilerFile inputFileContent) 
        {
            MD5 md5sum = MD5.Create();
            MemoryStream ms = GetCombineFileNames(inputFileContent)
                    .Select(x => _compiler.GetSourceFileNameFromRequestedFileName(x))
                    .Where(x => (x != null) && x.Exists)
                    .Select(x => x.LastWriteTimeUtc.Ticks)
                    .Aggregate(new MemoryStream(), (acc, x) => {
                                                       byte[] buf = BitConverter.GetBytes(x);
                                                       acc.Write(buf, 0, buf.Length);
                                                       return acc;
                                                   });
            return md5sum.ComputeHash(ms.GetBuffer()).Aggregate(new StringBuilder(), (acc, x) => {
                                                                                         acc.Append(x.ToString("x"));
                                                                                         return acc;
                                                                                     }).ToString();
        }

        private IEnumerable<ICompilerFile> GetCombineFileNames(ICompilerFile inputFileContent) {
            return inputFileContent.ReadLines()
                .Select(l => _lineRegex.Match(l))
                .Where(m => m.Success)
                .Select(m => inputFileContent.GetRelativeFile(m.Value));
        }
    }
}
