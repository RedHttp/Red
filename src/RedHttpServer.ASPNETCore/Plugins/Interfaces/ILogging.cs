using System;

namespace RedHttpServerCore.Plugins.Interfaces
{
    /// <summary>
    ///     Interface for classes used to log things
    /// </summary>
    public interface ILogging
    {
        /// <summary>
        ///     Create log entry with category and message
        /// </summary>
        /// <param name="category">The category of the log entry</param>
        /// <param name="message">The log message</param>
        void Log(string category, string message);

        /// <summary>
        ///     Create log entry with message only
        /// </summary>
        /// <param name="message">The log message</param>
        void Log(string message);

        /// <summary>
        ///     Create log entry from exception
        /// </summary>
        /// <param name="exception">Exception to log</param>
        void Log(Exception exception);

        /// <summary>
        ///     Whether the stacktrace should be included when logging exception
        /// </summary>
        bool IncludeStackTrace { get; set; }
    }
}