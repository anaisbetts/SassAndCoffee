namespace SassAndCoffee.Core {
    using System;

    public interface IProxy<T> : IDisposable {
        bool ReturnToPool { get; }
        Func<IProxy<T>, bool> OnDisposed { set; }
        T WrappedItem { get;  set; }
    }
}
