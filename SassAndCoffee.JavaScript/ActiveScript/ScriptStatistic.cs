namespace SassAndCoffee.JavaScript.ActiveScript {
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    [SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    public enum ScriptStatistic : uint {
        /// <summary>
        /// Return the number of statements executed since the script started or the statistics were reset.
        /// </summary>
        StatementCount = 1,
        InstructionCount = 2,
        InstructionTime = 3,
        TotalTime = 4,
    }
}
