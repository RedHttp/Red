using System;

namespace Red
{
    /// <summary>
    /// 
    /// </summary>
    public class HandlerExceptionEventArgs : EventArgs
    {
        /// <summary>
        ///    The exception that occured
        /// </summary>
        public readonly Exception Exception;

        /// <summary>
        ///    The endpoint path the exception occured on
        /// </summary>
        public readonly string Path;

        /// <summary>
        ///    The endpoint path the exception occured on
        /// </summary>
        public readonly string Method;

        internal HandlerExceptionEventArgs(string path, Exception exception)
        {
            Exception = exception;
            Path = path;
        }
    }
}