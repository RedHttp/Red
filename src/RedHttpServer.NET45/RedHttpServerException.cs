using System;

namespace RedHttpServerNet45
{
    /// <summary>
    /// Exception for errors in RedHttpServer
    /// </summary>
    [Serializable]
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