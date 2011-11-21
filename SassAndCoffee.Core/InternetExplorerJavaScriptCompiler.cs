namespace SassAndCoffee.Core {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using ComImports.ActiveScript;
    using ComImports.JavaScriptEngine;
    using V8Bridge.Interface;

    public class InternetExplorerJavaScriptCompiler : BaseActiveScriptSite, IV8ScriptCompiler, IDisposable {
        private IActiveScript _jsEngine;
        private IActiveScriptParseWrapper _jsParse;
        private object _jsDispatch;
        private Type _jsDispatchType;

        private Dictionary<string, object> _siteItems = new Dictionary<string, object>();

        private const string JavaScriptProgId = "JScript";
        public static bool IsSupported {
            get {
                return Type.GetTypeFromProgID(JavaScriptProgId) != null;
            }
        }

        public void InitializeLibrary(string libraryCode) {
            try {
                // Prefer Chakra
                _jsEngine = new ChakraJavaScriptEngine() as IActiveScript;
            } catch (Exception e) {
                // TODO: Make catch more specific
                _jsEngine = null;
            }

            if (_jsEngine == null) {
                // No need to catch here - engine of last resort
                _jsEngine = new JavaScriptEngine() as IActiveScript;
            }

            _jsEngine.SetScriptSite(this);
            _jsParse = new ActiveScriptParseWrapper(_jsEngine);
            _jsParse.InitNew();

            try {
                _jsParse.ParseScriptText(libraryCode, null, null, null, IntPtr.Zero, 0, ScriptTextFlags.IsVisible);
            } catch {
                var last = GetAndResetLastException();
                if (last != null)
                    throw last;
                else throw;
            }
            // Check for parse error
            var parseError = GetAndResetLastException();
            if (parseError != null)
                throw parseError;

            _jsEngine.GetScriptDispatch(null, out _jsDispatch);
            _jsDispatchType = _jsDispatch.GetType();
        }

        public string Compile(string function, string input) {
            try {
                return _jsDispatchType.InvokeMember(function, BindingFlags.InvokeMethod, null, _jsDispatch, new object[] { input }) as string;
            } catch {
                var last = GetAndResetLastException();
                if (last != null)
                    throw last;
                else throw;
            }
        }

        public override object GetItem(string name) {
            lock (_siteItems) {
                object result = null;
                return _siteItems.TryGetValue(name, out result) ? result : null;
            }
        }

        public override IntPtr GetTypeInfo(string name) {
            lock (_siteItems) {
                if (!_siteItems.ContainsKey(name))
                    return IntPtr.Zero;
                return Marshal.GetITypeInfoForType(_siteItems[name].GetType());
            }
        }

        ~InternetExplorerJavaScriptCompiler() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing) {
            ComRelease(ref _jsDispatch, !disposing);

            // For now these next two actually reference the same object, but it doesn't hurt to be explicit.
            ComRelease(ref _jsParse, !disposing);
            ComRelease(ref _jsEngine, !disposing);
        }

        private void ComRelease<T>(ref T o, bool final = false)
            where T : class {
            if (o != null && Marshal.IsComObject(o)) {
                if (final)
                    Marshal.FinalReleaseComObject(o);
                else
                    Marshal.ReleaseComObject(o);
            }
            o = null;
        }
    }
}
