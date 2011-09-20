using System;
using System.IO;

namespace SassAndCoffee.Core.Tests {
    internal class TestCompilerFile: ICompilerFile {
        private readonly string _fileName;
        private readonly string _content;
        private readonly DateTime _lastWriteTimeUtc;

        public TestCompilerFile(string fileName, string content) {
            this._fileName = fileName;
            this._content = content;
            _lastWriteTimeUtc = DateTime.UtcNow;
        }

        public DateTime LastWriteTimeUtc {
            get {
                AssertFileExists();
                return _lastWriteTimeUtc;
            }
        }

        public TextReader Open() 
        {
            AssertFileExists();
            return new StringReader(_content);
        }

        private void AssertFileExists() 
        {
            if (!Exists) {
                throw new FileNotFoundException();
            }
        }

        public string Name {
            get {
                return _fileName;
            }
        }

        public bool Exists {
            get {
                return _content != null;
            }
        }

        public ICompilerFile GetRelativeFile(string relativePath) 
        {
            // not really canonicalizing the real path, but that's not needed here
            return new TestCompilerFile(relativePath, null);
        }
    }
}
