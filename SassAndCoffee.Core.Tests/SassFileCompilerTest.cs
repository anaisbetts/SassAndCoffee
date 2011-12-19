namespace SassAndCoffee.Core.Tests {
    using System;
    using System.IO;
    using SassAndCoffee.Ruby.Sass;
    using Xunit;

    public class SassFileCompilerTest {
        [Fact]
        public void ScssSmokeTest() {
            var input = @"
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
            var actual = compileInput("test.scss", input);
            Assert.False(string.IsNullOrWhiteSpace(actual));
        }

        [Fact]
        public void SassNegativeSmokeTest() {
            var input = ".foo bar[val=\"//\"] { baz: bang; }";
            try {
                compileInput("test.sass", input);
            } catch (Exception e) {
                Assert.True(e.ToString().Contains("Syntax"));
            }
        }

        string compileInput(string filename, string input) {
            var fixture = new SassCompiler();

            using (var of = File.CreateText(filename)) {
                of.WriteLine(input);
            }

            try {

                // TODO: Fix this
                //     fixture.Init(TODO);
                string result = fixture.Compile(filename);
                Console.WriteLine(result);
                return result;
            } finally {
                File.Delete(filename);
            }
        }
    }
}
