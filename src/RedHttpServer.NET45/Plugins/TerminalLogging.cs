using System;
using RedHttpServerNet45.Plugins.Interfaces;

namespace RedHttpServerNet45.Plugins
{
    /// <summary>
    ///     Logs everything to terminal (standard output)
    /// </summary>
    public sealed class TerminalLogging : ILogging
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public void Log(string category, string message)
        {
            Console.WriteLine($"{DateTime.UtcNow:g}: {category} - {message}");
        }

        public void Log(string message)
        {
            Console.WriteLine($"{DateTime.UtcNow:g}: {message}");
        }

        public void Log(Exception exception)
        {
            Console.WriteLine(
                $"{DateTime.UtcNow:g}: {exception.GetType().Name} - {exception.Message}{(IncludeStackTrace ? $" Stack trace: \n{exception.StackTrace}\n" : "")}");
        }

        public bool IncludeStackTrace { get; set; } = false;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}