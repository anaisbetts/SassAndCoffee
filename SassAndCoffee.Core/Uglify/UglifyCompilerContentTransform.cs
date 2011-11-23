namespace SassAndCoffee.Core.Uglify {
    using System;
    using SassAndCoffee.Core.Pipeline;
    using SassAndCoffee.Core.Pooling;

    public class UglifyCompilerContentTransform : JavaScriptCompilerContentTransformBase {
        public override string InputMimeType {
            get { return "text/javascript"; }
        }

        public override string OutputMimeType {
            get { return "text/javascript"; }
        }

        public override void PreExecute(ContentTransformState state) {
            if (state.Path.EndsWith(".min.js", StringComparison.OrdinalIgnoreCase)) {
                state.Items.Add("Uglify", true);
                state.RemapPath(state.Path.Replace(".min.js", ".js"));
            }
        }

        public override void Execute(ContentTransformState state) {
            if (!state.Items.ContainsKey("Uglify"))
                return;

            base.Execute(state);
        }

        protected override IInstanceProvider<IJavaScriptCompiler> CreateCompilerProvider(
            IInstanceProvider<IJavaScriptRuntime> jsRuntimeProvider) {
            return new InstanceProvider<IJavaScriptCompiler>(
                () => new UglifyCompiler(jsRuntimeProvider));
        }
    }
}
