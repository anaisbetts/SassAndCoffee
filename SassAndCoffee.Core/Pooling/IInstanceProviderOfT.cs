namespace SassAndCoffee.Core {
    using System;

    public interface IInstanceProvider<T> {
        T GetInstance();
    }
}
