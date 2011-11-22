namespace SassAndCoffee.Core.Tests {
    using System;
    using SassAndCoffee.Core;
    using SassAndCoffee.Core.CoffeeScript;
    using SassAndCoffee.Core.Pooling;
    using Xunit;

    public class CoffeeScriptCompilerTest {
        [Fact]
        public void CoffeeScriptSmokeTest() {
            var input = @"v = x*5 for x in [1...10]";
            using (var fixture = new CoffeeScriptCompiler(new InstanceProvider<IJavaScriptRuntime>(
                () => new IEJavaScriptRuntime()))) {

                var result = fixture.Compile(input);
                Assert.False(String.IsNullOrWhiteSpace(result));
            }
        }

        [Fact]
        public void CoffeeScriptFailTest() {
            var input = "test.invlid.stuff/^/g!%%";
            using (var fixture = new CoffeeScriptCompiler(new InstanceProvider<IJavaScriptRuntime>(
                () => new IEJavaScriptRuntime()))) {

                bool shouldDie = false;

                try {
                    var result = fixture.Compile(input);
                    if (result.StartsWith("ENGINE FAULT"))
                        shouldDie = true;
                    else Console.WriteLine(result);
                } catch (Exception ex) {
                    Console.WriteLine("Ex: " + ex.Message);
                    shouldDie = true;
                }

                Assert.True(shouldDie);
            }
        }
    }
}