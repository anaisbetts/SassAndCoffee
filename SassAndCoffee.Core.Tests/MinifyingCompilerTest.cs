namespace SassAndCoffee.Core.Tests {
    using System;
    using SassAndCoffee.Core;
    using SassAndCoffee.Core.Pooling;
    using SassAndCoffee.Core.Uglify;
    using Xunit;

    public class MinifyingCompilerTest {
        [Fact]
        public void UglifySmokeTest() {
            using (var fixture = new UglifyCompiler(new InstanceProvider<IJavaScriptRuntime>(
                () => new IEJavaScriptRuntime()))) {
                var input = "var someLongVariableName = 4;";
                string output = fixture.Compile(input);

                Console.WriteLine("Input: '{0}', Output: '{1}'", input, output);
                Assert.True(output.Length < input.Length);
            }
        }
    }
}
