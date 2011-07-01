namespace SassAndCoffee.Core
{
    using SassAndCoffee.Core.Compilers;

    public interface ICompilerHost
    {
        /// <summary>
        /// The base file system path for the application
        /// </summary>
        string ApplicationBasePath { get; }

        /// <summary>
        /// Maps a path (including filename) to a compiler if one is available
        /// </summary>
        /// <param name="physicalPath">Path to the file to compile</param>
        /// <returns>Compiler if one is available, null otherwise</returns>
        ISimpleFileCompiler MapPathToCompiler(string physicalPath);

        /// <summary>
        /// Maps a url path to a physical file system path
        /// </summary>
        /// <param name="path">Url path</param>
        /// <returns>Absolute path to the file</returns>
        string MapPath(string path);
    }
}