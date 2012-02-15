namespace SassAndCoffee.JavaScript.ActiveScript {
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Thread interruption options.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
    [SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32"), Flags]
    public enum ScriptInterruptFlags : uint {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// If supported, enter the scripting engine's debugger at the current script execution point.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// If supported by the scripting engine's language, let the script handle the exception.
        /// Otherwise, the script method is aborted and the error code is returned to the caller; that
        /// is, the event source or macro invoker.
        /// </summary>
        RaiseException = 2,
    }
}
