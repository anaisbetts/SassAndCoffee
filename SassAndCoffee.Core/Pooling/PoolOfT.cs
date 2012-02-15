namespace SassAndCoffee.Core {
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// A simple pool that scales to always meet demand and retains a single instance when idle.
    /// </summary>
    /// <typeparam name="T">The type to pool.</typeparam>
    /// <typeparam name="TProxy">The type of the proxy.</typeparam>
    public class Pool<T, TProxy> : IInstanceProvider<TProxy>, IDisposable
        where T : class
        where TProxy : T, IProxy<T>, new() {

        private Func<T> _createPoolItem;
        private ConcurrentQueue<T> _pool = new ConcurrentQueue<T>();
        private bool _disposed = false;

        public Pool(Func<T> createInstance) {
            _createPoolItem = createInstance;
        }

        public Pool(IInstanceProvider<T> provider)
            : this(provider.GetInstance) { }

        public virtual TProxy GetInstance() {
            T poolItem;

            if (!_pool.TryDequeue(out poolItem)) {
                poolItem = _createPoolItem();
            }

            return new TProxy() {
                WrappedItem = poolItem,
                OnDisposed = ReturnToPool,
            };
        }

        private bool ReturnToPool(IProxy<T> proxy) {
            // Depopulate a disposed pool
            if (_disposed || proxy == null || !proxy.ReturnToPool)
                return true;

            // Repopulate an empty pool
            if (_pool.IsEmpty) {
                _pool.Enqueue(proxy.WrappedItem);
                return false;
            }

            // Depopulate a full pool
            return true;
        }

        public void Dispose() {
            _disposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_pool != null) {
                    foreach (var item in _pool) {
                        var disposable = item as IDisposable;
                        if (disposable != null) disposable.Dispose();
                    }
                    _pool = null;
                }
            }
        }
    }
}
