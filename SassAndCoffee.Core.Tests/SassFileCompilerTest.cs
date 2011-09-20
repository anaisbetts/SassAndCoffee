namespace SassAndCoffee.Core.Tests
{
    using System;
    using System.IO;

    using SassAndCoffee.Core.Compilers;

    using Xunit;

    using SassAndCoffee.Core;

    public class SassFileCompilerTest
    {
        [Fact]
        public void ScssSmokeTest()
        {
            var input = ".foo bar[val=\"//\"] { baz: bang; }";
            Assert.False(String.IsNullOrWhiteSpace(compileInput("test.scss", input)));
        }
    
        [Fact]
        public void SassNegativeSmokeTest()
        {
            var input = ".foo bar[val=\"//\"] { baz: bang; }";
            var output = compileInput("test.sass", input);

            Assert.True(output.Contains("Syntax"));
        }

        string compileInput(string filename, string input)
        {
            var fixture = new SassFileCompiler();

            try {
                string result = fixture.ProcessFileContent(new TestCompilerFile(filename, input));
                Console.WriteLine(result);
                return result;
            } finally {
                File.Delete(filename);
            }
        }
    }
}
