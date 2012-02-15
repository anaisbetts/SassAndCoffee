using System.Diagnostics.CodeAnalysis;
namespace SassAndCoffee.JavaScript.ActiveScript {

    /// <summary>
    /// Signifies a special thread or class of threads.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    [SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    public enum ScriptThreadId : uint {
        /// <summary>
        /// The currently executing thread.
        /// </summary>
        Current = 0xFFFFFFFD,

        /// <summary>
        /// The base thread; that is, the thread in which the scripting engine was instantiated.
        /// </summary>
        Base = 0xFFFFFFFE,

        /// <summary>
        /// All threads.
        /// </summary>
        All = 0xFFFFFFFF
    }
}
