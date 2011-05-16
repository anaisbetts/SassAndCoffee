using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace SassAndCoffee.Tests
{
    public class CoffeeScriptCompilerTest
    {
        [Fact]
        public void CoffeeScriptSmokeTest()
        {
            var input = @"v = x*5 for x in [1...10]";
            var fixture = new CoffeeScriptCompiler();

            var result = fixture.Compile(input);
            Assert.False(String.IsNullOrWhiteSpace(result));
        }
    }
}
