namespace SassAndCoffee.Core.Pooling {
    using System;

    public interface IInstanceProvider<T>
        where T : IDisposable {
        T GetInstance();
    }
}
