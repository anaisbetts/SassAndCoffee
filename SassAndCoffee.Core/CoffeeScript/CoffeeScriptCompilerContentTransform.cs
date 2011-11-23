namespace SassAndCoffee.Core.CoffeeScript {
    using SassAndCoffee.Core.Pipeline;
    using SassAndCoffee.Core.Pooling;

    public class CoffeeScriptCompilerContentTransform : JavaScriptCompilerContentTransformBase {
        public override string InputMimeType {
            get { return "text/coffeescript"; }
        }

        public override string OutputMimeType {
            get { return "text/javascript"; }
        }

        protected override IInstanceProvider<IJavaScriptCompiler> CreateCompilerProvider(
            IInstanceProvider<IJavaScriptRuntime> jsRuntimeProvider) {
            return new InstanceProvider<IJavaScriptCompiler>(
                () => new CoffeeScriptCompiler(jsRuntimeProvider));
        }
    }
}
