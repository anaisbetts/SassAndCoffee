namespace SassAndCoffee.Core.Sass {
    using Microsoft.Scripting;
    using Microsoft.Scripting.Hosting;

    public class SassCompilerScriptHost : ScriptHost {

        private PlatformAdaptationLayer _pal;

        public SassCompilerScriptHost(PlatformAdaptationLayer pal) {
            _pal = pal;
        }

        public override PlatformAdaptationLayer PlatformAdaptationLayer { get { return _pal; } }
    }
}
