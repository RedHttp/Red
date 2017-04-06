namespace RHttpServer.Logging
{
    /// <summary>
    ///     How the logger should handle calls to Log(..)
    /// </summary>
    public enum LoggingOption
    {
        /// <summary>
        ///     Perform no logging whatsoever
        /// </summary>
        None,

        /// <summary>
        ///     Write the logged items to the terminal (standard output)
        /// </summary>
        Terminal,

        /// <summary>
        ///     Write the logged items to a file.
        ///     Do not forget to set the file path.
        /// </summary>
        File
    }
}