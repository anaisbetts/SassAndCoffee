namespace SassAndCoffee.AspNet {
    using System.Collections.Generic;

    public class SassInterceptorModule : PathBasedHandlerRemapper<SassHandler> {
        private static readonly string[] Extensions = { ".css" };
        public override IEnumerable<string> HandledExtensions { get { return Extensions; } }
    }
}
