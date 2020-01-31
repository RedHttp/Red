using System;

namespace Red
{
    /// <summary>
    /// </summary>
    public class HandlerExceptionEventArgs : EventArgs
    {
        /// <summary>
        ///     The exception that occured
        /// </summary>
        public readonly Exception Exception;

        /// <summary>
        ///     The endpoint path the exception occured on
        /// </summary>
        public readonly string Method;

        /// <summary>
        ///     The endpoint path the exception occured on
        /// </summary>
        public readonly string Path;

        internal HandlerExceptionEventArgs(string method, string path, Exception exception)
        {
            Method = method;
            Path = path;
            Exception = exception;
        }
    }
}