namespace SassAndCoffee.AspNet {
    using System.Collections.Generic;
    using SassAndCoffee.Core.CoffeeScript;
    using SassAndCoffee.Core.Pipeline;
    using SassAndCoffee.Core.Uglify;

    public class JavaScriptInterceptorModule : PathBasedHandlerRemapper {
        public override IEnumerable<string> HandledExtensions {
            get { return new string[] { ".js" }; }
        }

        public override IEnumerable<IContentTransform> Transformations {
            get {
                return new IContentTransform[] {
                    new FileSourceContentTransform("text/javascript", ".js"),
                    new JavaScriptCombineContentTransform(),
                    new FileSourceContentTransform("text/coffeescript", ".coffee"),
                    new CoffeeScriptCompilerContentTransform(),
                    new UglifyCompilerContentTransform(),
                };
            }
        }
    }
}
