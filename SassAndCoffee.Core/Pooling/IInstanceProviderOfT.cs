namespace SassAndCoffee.Core {
    using System.Diagnostics.CodeAnalysis;

    public interface IInstanceProvider<T> {
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "It could make a new instance every time.")]
        T GetInstance();
    }
}
