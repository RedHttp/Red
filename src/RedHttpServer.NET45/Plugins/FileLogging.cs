using System;
using System.IO;
using RedHttpServerNet45.Plugins.Interfaces;

namespace RedHttpServerNet45.Plugins
{
    /// <summary>
    ///     Logs everything to a specified file
    /// </summary>
    public sealed class FileLogging : ILogging
    {
        /// <summary>
        ///     Logs everything to a specified file
        /// </summary>
        /// <param name="filename">file to log entries to</param>
        public FileLogging(string filename)
        {
            LogFile = filename;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public string LogFile { get; }

        public void Log(string category, string message)
        {
            File.AppendAllText(LogFile, $"{DateTime.UtcNow:g}: {category} - {message}\n");
        }

        public void Log(string message)
        {
            File.AppendAllText(LogFile, $"{DateTime.UtcNow:g}: {message}\n");
        }

        public void Log(Exception exception)
        {
            File.AppendAllText(LogFile,
                $"{DateTime.UtcNow:g}: {exception.GetType().Name} - {exception.Message}{(IncludeStackTrace ? $" Stack trace: \n{exception.StackTrace}" : "")}\n");
        }

        public bool IncludeStackTrace { get; set; } = false;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}