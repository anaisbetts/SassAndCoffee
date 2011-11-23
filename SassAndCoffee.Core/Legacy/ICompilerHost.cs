namespace SassAndCoffee.Core {

    public interface ICompilerHost
    {
        /// <summary>
        /// The base file system path for the application
        /// </summary>
        string ApplicationBasePath { get; }

        /// <summary>
        /// Maps a url path to a physical file system path
        /// </summary>
        /// <param name="path">Url path</param>
        /// <returns>Absolute path to the file</returns>
        string MapPath(string path);
    }
}