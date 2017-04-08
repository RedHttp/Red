using System;
using System.IO;
using System.Text;

namespace RedHttpServer.Logging
{
    /// <summary>
    ///     Class used to log things easily
    /// </summary>
    public static class Logger
    {
        private static LoggingOption _logOpt = LoggingOption.None;
        private static string _logFilePath;
        private static bool _stackTrace;
        private static readonly object FileLock = new object();

        /// <summary>
        ///     Method used to configure the logger.
        ///     By default the logger is set to LoggingOption.None
        /// </summary>
        /// <param name="logOpt">How (and if) logging should be performed</param>
        /// <param name="includeStackTrace">Whether to include the stack trace when logging exceptions</param>
        /// <param name="logFilePath">Where the log file is stored, if LoggingOption.File is chosen</param>
        public static void Configure(LoggingOption logOpt, bool includeStackTrace, string logFilePath = "")
        {
            _logOpt = logOpt;
            _stackTrace = includeStackTrace;
            _logFilePath = logFilePath;
            if (logOpt != LoggingOption.File) return;
            if (string.IsNullOrWhiteSpace(logFilePath))
                _logFilePath = "LOG.txt";
            else if ((Directory.Exists(logFilePath) || File.Exists(logFilePath)) &&
                     File.GetAttributes(logFilePath).HasFlag(FileAttributes.Directory))
                _logFilePath = Path.Combine(_logFilePath, "LOG.txt");
        }

        /// <summary>
        ///     Logs an exception using the current LoggingOption.
        ///     Will include stacktrace if selected.
        /// </summary>
        /// <param name="ex"></param>
        public static void Log(Exception ex)
        {
            switch (_logOpt)
            {
                case LoggingOption.None:
                    break;
                case LoggingOption.Terminal:
                    Console.WriteLine(
                        $"{DateTime.Now:g}: {ex.GetType().Name} - {ex.Message}{(_stackTrace ? $"\n Stack trace:\n{ex.StackTrace}\n" : "")}");
                    break;
                case LoggingOption.File:
                    lock (FileLock)
                    {
                        File.AppendAllText(_logFilePath,
                            $"{DateTime.Now:g}: {ex.GetType().Name} - {ex.Message}{(_stackTrace ? $"\n Stack trace:\n{ex.StackTrace}\n" : "")}",
                            Encoding.Default);
                    }
                    break;
            }
        }

        /// <summary>
        ///     Log an item using title and message using the current LoggingOption.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public static void Log(string title, string message)
        {
            switch (_logOpt)
            {
                case LoggingOption.None:
                    break;
                case LoggingOption.Terminal:
                    Console.WriteLine($"{DateTime.Now:g}: {title} - {message}");
                    break;
                case LoggingOption.File:
                    lock (FileLock)
                    {
                        File.AppendAllText(_logFilePath, $"{DateTime.Now:g}: {title} - {message}\n",
                            Encoding.Default);
                    }
                    break;
            }
        }

        /// <summary>
        ///     Log an item message using the current LoggingOption.
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            switch (_logOpt)
            {
                case LoggingOption.None:
                    break;
                case LoggingOption.Terminal:
                    Console.WriteLine($"{DateTime.Now:g}: {message}");
                    break;
                case LoggingOption.File:
                    lock (FileLock)
                    {
                        File.AppendAllText(_logFilePath, $"{DateTime.Now:g}: {message}\n",
                            Encoding.Default);
                    }
                    break;
            }
        }
    }
}