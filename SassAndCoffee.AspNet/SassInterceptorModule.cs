namespace SassAndCoffee.AspNet {
    using SassAndCoffee.Ruby.Sass;

    /// <summary>
    /// Conditionally handles .css requests with the Sass content pipeline.
    /// </summary>
    public class SassInterceptorModule : PathBasedHandlerRemapper {
        public SassInterceptorModule()
            : base(".css", new SassCompilerContentTransform()) {
        }
    }
}
