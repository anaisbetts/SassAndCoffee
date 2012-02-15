namespace SassAndCoffee.AspNet {
    using System.Diagnostics.CodeAnalysis;
    using SassAndCoffee.Ruby.Sass;

    /// <summary>
    /// Conditionally handles .css requests with the Sass content pipeline.
    /// </summary>
    public class SassInterceptorModule : PathBasedHandlerRemapper {
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Base class takes care of it.")]
        public SassInterceptorModule()
            : base(".css", new SassCompilerContentTransform()) {
        }
    }
}
