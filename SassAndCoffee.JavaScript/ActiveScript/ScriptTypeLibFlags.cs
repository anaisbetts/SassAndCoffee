namespace SassAndCoffee.JavaScript.ActiveScript {
    using System;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
    [SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    [Flags]
    public enum ScriptTypeLibFlags : uint {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// The type library describes an ActiveX control used by the host.
        /// </summary>
        IsControl = 0x00000010,

        /// <summary>
        /// Not documented.
        /// </summary>
        IsPersistent = 0x00000040,
    }
}
