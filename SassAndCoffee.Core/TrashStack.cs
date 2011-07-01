using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SassAndCoffee
{
    public sealed class ValueContainer<T> : IDisposable
    {
        IDisposable _inner;

        internal ValueContainer(T value, Action<ValueContainer<T>> disposer)
        {
            Value = value;
            _inner = Disposable.Create(() => disposer(this));
        }

        public T Value { get; internal set; }

        public void Dispose()
        {
            _inner.Dispose();
        }
    }

    public class TrashStack<T>
    {
        readonly int _maxFreeItems;
        readonly Func<T> _valueFactory;
        readonly ConcurrentStack<ValueContainer<T>> _availableItems = new ConcurrentStack<ValueContainer<T>>();

        public TrashStack(Func<T> valueFactory, int maxFreeItems = 10)
        {
            _valueFactory = valueFactory;
            _maxFreeItems = maxFreeItems;
        }

        public ValueContainer<T> Get()
        {
            ValueContainer<T> ret;
            if (_availableItems.TryPop(out ret)) {
                return ret;
            }

            ret = new ValueContainer<T>(_valueFactory(), Release);
            return ret;
        }

        void Release(ValueContainer<T> obj)
        {
            _availableItems.Push(new ValueContainer<T>(obj.Value, Release));

            ValueContainer<T> dontcare;
            while (_availableItems.Count > _maxFreeItems) {
                _availableItems.TryPop(out dontcare);
            }
        }
    }

    public static class Disposable
    {
        sealed class ActionDisposable : IDisposable
        {
            Action _block;
            public ActionDisposable(Action block)
            {
                _block = block;
            }

            public void Dispose()
            {
                _block();
            }
        }

        public static IDisposable Create(Action block)
        {
            return new ActionDisposable(block);
        }
    }
}