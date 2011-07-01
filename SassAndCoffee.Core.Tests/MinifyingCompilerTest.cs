namespace SassAndCoffee.Core.Tests
{
    using System;

    using Xunit;

    using SassAndCoffee.Core;

    public class MinifyingCompilerTest
    {
        [Fact]
        public void UglifySmokeTest()
        {
            var fixture = new MinifyingCompiler();
            var input = "var someLongVariableName = 4;";
            string output = fixture.Compile(input);

            Console.WriteLine("Input: '{0}', Output: '{1}'", input, output);
            Assert.True(output.Length < input.Length);
        }
    }
}
