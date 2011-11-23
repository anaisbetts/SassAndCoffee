namespace SassAndCoffee.Core.Pipeline {
    using SassAndCoffee.Core.Pooling;

    public abstract class JavaScriptCompilerContentTransformBase : ContentTransformBase {

        private IInstanceProvider<IJavaScriptRuntime> _jsRuntimeProvider;
        private IInstanceProvider<IJavaScriptCompiler> _jsCompilerProvider;

        public abstract string InputMimeType { get; }
        public abstract string OutputMimeType { get; }

        public JavaScriptCompilerContentTransformBase() {
            _jsRuntimeProvider = new InstanceProvider<IJavaScriptRuntime>(
                () => new IEJavaScriptRuntime());
            _jsCompilerProvider = CreateCompilerProvider(_jsRuntimeProvider);
        }

        public override void Execute(ContentTransformState state) {
            // If input is empty or the wrong type, do nothing
            if (state.Content == null || state.MimeType != InputMimeType)
                return;

            string result = null;
            using (var compiler = _jsCompilerProvider.GetInstance()) {
                result = compiler.Compile(state.Content);
            }

            if (result != null) {
                state.ReplaceContent(new ContentResult() {
                    Content = result,
                    MimeType = OutputMimeType,
                });
            }
        }


        protected abstract IInstanceProvider<IJavaScriptCompiler> CreateCompilerProvider(
            IInstanceProvider<IJavaScriptRuntime> jsRuntimeProvider);
    }
}
