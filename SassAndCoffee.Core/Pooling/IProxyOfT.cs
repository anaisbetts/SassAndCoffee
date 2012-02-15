namespace SassAndCoffee.Core {
    using System;
    using System.Diagnostics.CodeAnalysis;

    public interface IProxy<T> : IDisposable {
        bool ReturnToPool { get; }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This works fine.")]
        Func<IProxy<T>, bool> OnDisposed { get; set; }

        T WrappedItem { get; set; }
    }
}
