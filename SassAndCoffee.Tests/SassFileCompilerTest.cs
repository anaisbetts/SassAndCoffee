using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace SassAndCoffee
{
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

            using(var of = File.CreateText(filename)) {
                of.WriteLine(input);
            }

            try {
                fixture.Init(null);
                string result = fixture.ProcessFileContent(filename);
                Console.WriteLine(result);
                return result;
            } finally {
                File.Delete(filename);
            }
        }
    }
}
