﻿namespace SassAndCoffee.Core {
    using System;

    public sealed class InstanceProvider<T> : IInstanceProvider<T>
        where T : IDisposable {
        private Func<T> _createInstance;

        public InstanceProvider(Func<T> createInstance) {
            _createInstance = createInstance;
        }

        public T GetInstance() {
            return _createInstance();
        }
    }
}
