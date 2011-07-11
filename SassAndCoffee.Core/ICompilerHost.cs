namespace SassAndCoffee.Core
{
    using SassAndCoffee.Core.Compilers;

    public interface ICompilerHost
    {
        /// <summary>
        /// The base file system path for the application
        /// </summary>
        string ApplicationBasePath { get; }
    }
}