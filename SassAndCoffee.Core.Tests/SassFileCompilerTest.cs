namespace SassAndCoffee.Core.Tests {
    using System;
    using System.IO;
    using SassAndCoffee.Ruby.Sass;
    using Xunit;

    public class SassFileCompilerTest {
        private readonly ISassCompiler _fixture;
        private const string GOOD_SCSS_INPUT = @"
// SCSS

.error {
  border: 1px #f00;
  background: #fdd;
}
.error.intrusion {
  font-size: 1.3em;
  font-weight: bold;
}

.badError {
  @extend .error;
  border-width: 3px;
}
";

        private const string BAD_SASS_INPUT = ".foo bar[val=\"//\"] { baz: bang; }";

        public SassFileCompilerTest() {
            _fixture = new SassCompiler();
        }

        [Fact]
        public void ScssSmokeTest() {
            var actual = compileInput("test.scss", GOOD_SCSS_INPUT);
            Assert.False(string.IsNullOrWhiteSpace(actual));
        }

        [Fact]
        public void SassNegativeSmokeTest() {
            try {
                compileInput("test.sass", BAD_SASS_INPUT);
            } catch (Exception e) {
                Assert.True(e.ToString().Contains("Syntax"));
            }
        }

        [Fact]
        public void ScssCompileAsStringTest() {
            var actual = _fixture.CompileScss(GOOD_SCSS_INPUT, true);
            Assert.False(string.IsNullOrWhiteSpace(actual));
        }

        [Fact]
        public void SassNegativeCompileAsStringTest() {
            try {
                _fixture.CompileSass(BAD_SASS_INPUT, true);
            } catch (Exception e) {
                Assert.True(e.ToString().Contains("Syntax"));
            }
        }

        string compileInput(string filename, string input) {
            using (var of = File.CreateText(filename)) {
                of.WriteLine(input);
            }

            try {

                // TODO: Fix this
                //     fixture.Init(TODO);
                string result = _fixture.Compile(filename, true, null);
                Console.WriteLine(result);
                return result;
            } finally {
                File.Delete(filename);
            }
        }
    }
}
