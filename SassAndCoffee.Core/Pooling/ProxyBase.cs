namespace SassAndCoffee.Core {
    using System;

    public class ProxyBase<T> : IProxy<T>
        where T : class {
        public Func<IProxy<T>, bool> OnDisposed { get; set; }
        public bool ReturnToPool { get; protected set; }
        public T WrappedItem {
            get {
                if (_disposed)
                    throw new ObjectDisposedException("This proxy wrapper has been disposed and cannot be used again.");
                return _wrappedItem;
            }
            set {
                if (_disposed)
                    throw new ObjectDisposedException("This proxy wrapper has been disposed and cannot be used again.");
                if (_wrappedItem != null)
                    throw new InvalidOperationException("WrappedItem can only be set once.");
                _wrappedItem = value;
            }
        }

        private bool _disposed = false;
        private T _wrappedItem;

        public ProxyBase() {
            ReturnToPool = true;
        }

        public ProxyBase(T wrapped)
            : this() {
            _wrappedItem = wrapped;
        }

        public void Dispose() {
            if ((OnDisposed == null || OnDisposed(this)) && WrappedItem != null) {
                var disposable = WrappedItem as IDisposable;
                if(disposable != null) disposable.Dispose();
            }
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            // Whether or not we disposed of the wrapped item, it's no longer ours to use.
            _wrappedItem = null;
            _disposed = true;
        }
    }
}
