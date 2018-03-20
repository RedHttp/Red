using System;

namespace Red
{
    /// <summary>
    /// Exception for errors in RedHttpServer
    /// </summary>
    public class RedHttpServerException : Exception
    {
        internal RedHttpServerException(string message) : base(message)
        {
        }
    }
}