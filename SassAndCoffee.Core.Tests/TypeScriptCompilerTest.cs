namespace SassAndCoffee.Core.Tests {
    using System;
    using SassAndCoffee.Core;
    using SassAndCoffee.JavaScript;
    using SassAndCoffee.JavaScript.TypeScript;
    using Xunit;

    public class TypeScriptCompilerTest {
        [Fact]
        public void TypeScriptSmokeTest() {
            var input = @"var foo:number = 5;";
            using (var fixture = new TypeScriptCompiler(new InstanceProvider<IJavaScriptRuntime>(
                () => new IEJavaScriptRuntime()))) {

                var result = fixture.Compile(input);
                Assert.False(String.IsNullOrWhiteSpace(result));
            }
        }

        [Fact]
        public void TypeScriptFailTest() {
            var input = "test.invlid.stuff/^/g!%%";
            using (var fixture = new TypeScriptCompiler(new InstanceProvider<IJavaScriptRuntime>(
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