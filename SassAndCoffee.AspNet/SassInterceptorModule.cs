namespace SassAndCoffee.AspNet {
    using System.Collections.Generic;
    using SassAndCoffee.Core.Pipeline;
    using SassAndCoffee.Core.Sass;

    public class SassInterceptorModule : PathBasedHandlerRemapper {
        public override IEnumerable<string> HandledExtensions {
            get { return new string[] { ".css" }; }
        }

        public override IEnumerable<IContentTransform> Transformations {
            get { return new IContentTransform[] { new SassCompilerContentTransform() }; }
        }
    }
}
