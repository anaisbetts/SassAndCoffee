using System.Diagnostics.CodeAnalysis;
namespace SassAndCoffee.JavaScript.ActiveScript {

    /// <summary>
    /// Contains named constant values that specify the state of a thread in a scripting
    /// engine. This enumeration is used by the IActiveScript::GetScriptThreadState method.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    public enum ScriptThreadState : uint {
        /// <summary>
        /// Specified thread is not currently servicing a scripted event, processing
        /// immediately executed script text, or running a script macro.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Scriipt")]
        NotInScriipt = 0,

        /// <summary>
        /// Specified thread is actively servicing a scripted event, processing
        /// immediately executed script text, or running a script macro.
        /// </summary>
        Running = 1,
    }
}
