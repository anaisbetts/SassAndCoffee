namespace SassAndCoffee.JavaScript {
    using System;
    using SassAndCoffee.Core;

    public abstract class JavaScriptCompilerContentTransformBase : IContentTransform {
        private readonly IInstanceProvider<IJavaScriptCompiler> _jsCompilerProvider;

        public string InputMimeType { get; private set; }
        public string OutputMimeType { get; private set; }

        protected JavaScriptCompilerContentTransformBase(
            string inputMimeType,
            string outputMimeType,
            IInstanceProvider<IJavaScriptCompiler> jsCompilerProvider) {
            InputMimeType = inputMimeType;
            OutputMimeType = outputMimeType;
            _jsCompilerProvider = jsCompilerProvider;
        }

        public abstract void PreExecute(ContentTransformState state);
        public abstract void Execute(ContentTransformState state);

        protected virtual void ExecuteWithArguments(ContentTransformState state, params object[] args) {
            if (state == null)
                throw new ArgumentNullException("state");

            // If input is empty or the wrong type, do nothing
            if (state.Content == null || state.MimeType != InputMimeType)
                return;

            string result;
            using (var compiler = _jsCompilerProvider.GetInstance()) {
                result = compiler.Compile(state.Content, args);
            }

            if (result != null) {
                state.ReplaceContent(new ContentResult {
                    Content = result,
                    MimeType = OutputMimeType,
                });
            }
        }
    }
}
