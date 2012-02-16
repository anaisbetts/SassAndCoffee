namespace SassAndCoffee.JavaScript.Uglify {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using SassAndCoffee.Core;

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Uglify")]
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

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public override void PreExecute(ContentTransformState state) {
            if (state == null)
                throw new ArgumentNullException("state");

            if (state.Path.EndsWith(".min.js", StringComparison.OrdinalIgnoreCase)) {
                state.Items.Add(StateKey, true);
                var newPath = state.Path
                    .ToLowerInvariant()
                    .Replace(".min.js", ".js");
                state.RemapPath(newPath);
            }
        }

        public override void Execute(ContentTransformState state) {
            if (state == null)
                throw new ArgumentNullException("state");

            if (!state.Items.ContainsKey(StateKey))
                return;
            base.ExecuteWithArguments(state);
        }
    }
}
