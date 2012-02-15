namespace SassAndCoffee.JavaScript.Uglify {
    using System;
    using SassAndCoffee.Core;

    public class UglifyCompilerContentTransform : JavaScriptCompilerContentTransformBase {
        public const string StateKey = "Uglify";

        [Obsolete("This constructor is present for backwards compatibility with existing libraries. Do not use for new development.", true)]
        public UglifyCompilerContentTransform()
            : this(new InstanceProvider<IJavaScriptRuntime>(() => new IEJavaScriptRuntime())) { }

        public UglifyCompilerContentTransform(IInstanceProvider<IJavaScriptRuntime> jsRuntimeProvider)
            : base(
                "text/javascript",
                "text/javascript",
                new InstanceProvider<IJavaScriptCompiler>(() => new UglifyCompiler(jsRuntimeProvider))) { }

        public override void PreExecute(ContentTransformState state) {
            if (state.Path.EndsWith(".min.js", StringComparison.OrdinalIgnoreCase)) {
                state.Items.Add(StateKey, true);
                var newPath = state.Path
                    .ToLowerInvariant()
                    .Replace(".min.js", ".js");
                state.RemapPath(newPath);
            }
        }

        public override void Execute(ContentTransformState state) {
            if (!state.Items.ContainsKey(StateKey))
                return;
            base.Execute(state, null);
        }
    }
}
