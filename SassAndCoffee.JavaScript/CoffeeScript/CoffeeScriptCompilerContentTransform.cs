namespace SassAndCoffee.JavaScript.CoffeeScript {
    using System;
    using System.Text.RegularExpressions;
    using SassAndCoffee.Core;
    using System.Diagnostics.CodeAnalysis;

    public class CoffeeScriptCompilerContentTransform : JavaScriptCompilerContentTransformBase {
        public const string StateKeyBare = "CoffeeScript_Bare";

        public static readonly Regex BareModeDetection = new Regex(
            @"\.bare(\.min)?\.js$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        [Obsolete("This constructor is present for backwards compatibility with existing libraries. Do not use for new development.", true)]
        public CoffeeScriptCompilerContentTransform()
            : this(new InstanceProvider<IJavaScriptRuntime>(() => new IEJavaScriptRuntime())) { }

        public CoffeeScriptCompilerContentTransform(IInstanceProvider<IJavaScriptRuntime> jsRuntimeProvider)
            : base(
                "text/coffeescript",
                "text/javascript",
                new InstanceProvider<IJavaScriptCompiler>(() => new CoffeeScriptCompiler(jsRuntimeProvider))) { }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public override void PreExecute(ContentTransformState state) {
            if (state == null)
                throw new ArgumentNullException("state");

            if (BareModeDetection.IsMatch(state.Path)) {
                state.Items.Add(StateKeyBare, true);
                var newPath = state.Path
                    .ToLowerInvariant()
                    .Replace(".bare.js", ".js")
                    .Replace(".bare.min.js", ".min.js");
                state.RemapPath(newPath);
            }
        }

        public override void Execute(ContentTransformState state) {
            if (state == null)
                throw new ArgumentNullException("state");

            // Default to wrapped mode like CoffeeScript compiler
            bool bare = state.Items.ContainsKey(StateKeyBare);
            Execute(state, bare);
        }
    }
}
