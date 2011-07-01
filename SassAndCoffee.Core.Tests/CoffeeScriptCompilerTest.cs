namespace SassAndCoffee.Core.Tests
{
    using System;

    using Xunit;

    using SassAndCoffee.Core;

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

        [Fact]
        public void CoffeeScriptFailTest()
        {
            var input = "@#)$(@#)(@#_$)(@_)@ !!@_@@@ window.alert \"foo\" if 3>2 else if else if";
            var fixture = new CoffeeScriptCompiler();

            bool shouldDie = true;
            
            try {
                var result = fixture.Compile(input);
                Console.WriteLine(result);
            } catch(Exception ex) {
                Console.WriteLine("Ex: " + ex.Message);
                shouldDie = false;
            }

            Assert.False(shouldDie);
        }

        [Fact]
        public void V8CompilerLoadTest()
        {
            var fixture = JS.CreateJavascriptCompiler();

            Assert.False(fixture is JurassicCompiler);
        }
    }
}