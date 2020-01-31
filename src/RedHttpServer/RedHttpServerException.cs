using System;

namespace Red
{
    /// <inheritdoc />
    /// <summary>
    ///     Exception for errors in RedHttpServer
    /// </summary>
    public class RedHttpServerException : Exception
    {
        internal RedHttpServerException(string message) : base(message)
        {
        }
    }
}