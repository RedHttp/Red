using System;

namespace RHttpServer
{
    public class RHttpServerException : Exception
    {
        internal RHttpServerException(string msg) : base(msg)
        {
        }

        internal RHttpServerException()
        {
        }
    }
}