namespace SassAndCoffee.JavaScript {
    using System;
    using System.Collections.Generic;
    using SassAndCoffee.Core;

    public abstract class JavaScriptCompilerBase : IJavaScriptCompiler {

        public abstract IEnumerable<string> CompilerLibraryResourceNames { get; }
        public abstract string CompilationFunctionName { get; }

        private readonly IInstanceProvider<IJavaScriptRuntime> _jsRuntimeProvider;
        private IJavaScriptRuntime _js;
        private bool _initialized = false;
        private readonly object _lock = new object();

        protected JavaScriptCompilerBase(IInstanceProvider<IJavaScriptRuntime> jsRuntimeProvider) {
            _jsRuntimeProvider = jsRuntimeProvider;
        }

        public string Compile(string source, params object[] args) {
            if (source == null)
                throw new ArgumentException("source cannot be null.", "source");

            object[] compileArgs;
            if (args != null && args.Length > 0) {
                compileArgs = new object[args.Length + 1];
                compileArgs[0] = source;
                args.CopyTo(compileArgs, 1);
            } else {
                compileArgs = new object[] { source };
            }

            lock (_lock) {
                Initialize();
                return _js.ExecuteFunction<string>(CompilationFunctionName, compileArgs);
            }
        }

        private void Initialize() {
            if (!_initialized) {
                _js = _jsRuntimeProvider.GetInstance();
                _js.Initialize();
                foreach (var library in CompilerLibraryResourceNames) {
                    _js.LoadLibrary(Utility.ResourceAsString(library, GetType()));
                }
                _initialized = true;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_js != null) {
                    _js.Dispose();
                    _js = null;
                }
            }
        }
    }
}
