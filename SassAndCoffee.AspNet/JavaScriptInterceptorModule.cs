namespace SassAndCoffee.AspNet {
    using SassAndCoffee.Core;
    using SassAndCoffee.JavaScript;
    using SassAndCoffee.JavaScript.CoffeeScript;
    using SassAndCoffee.JavaScript.Uglify;

    /// <summary>
    /// Conditionally handles .js requests with the combine + coffeescript + uglify content pipeline.
    /// </summary>
    public class JavaScriptInterceptorModule : PathBasedHandlerRemapper {
        public JavaScriptInterceptorModule()
            : this(new InstanceProvider<IJavaScriptRuntime>(() => new IEJavaScriptRuntime())) { }

        public JavaScriptInterceptorModule(IInstanceProvider<IJavaScriptRuntime> jsRuntimeProvider)
            : base(
                ".js",
                new FileSourceContentTransform("text/javascript", ".js"),
                new JavaScriptCombineContentTransform(),
                new FileSourceContentTransform("text/coffeescript", ".coffee"),
                new CoffeeScriptCompilerContentTransform(jsRuntimeProvider),
                new UglifyCompilerContentTransform(jsRuntimeProvider)) {
        }
    }
}
