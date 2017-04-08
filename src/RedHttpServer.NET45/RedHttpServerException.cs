using System;

namespace RedHttpServer
{
    /// <summary>
    /// Exception for errors in RedHttpServer
    /// </summary>
    public class RedHttpServerException : Exception
    {
        internal RedHttpServerException(string msg) : base(msg)
        {
        }

        internal RedHttpServerException()
        {
        }
    }
}