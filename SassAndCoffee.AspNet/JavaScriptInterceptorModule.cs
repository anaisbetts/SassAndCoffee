namespace SassAndCoffee.AspNet {
    using System.Collections.Generic;

    public class JavaScriptInterceptorModule : PathBasedHandlerRemapper<JavaScriptHandler> {
        private static readonly string[] Extensions = { ".js" };
        public override IEnumerable<string> HandledExtensions { get { return Extensions; } }
    }
}
