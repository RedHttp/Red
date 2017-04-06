using System.Net;

namespace RHttpServer
{
    /// <summary>
    ///     The interface that must be implemented for a class to be able to handle http security
    /// </summary>
    public interface IHttpSecurityHandler
    {
        /// <summary>
        ///     The security settings
        /// </summary>
        IHttpSecuritySettings Settings { get; set; }

        /// <summary>
        ///     The method for monitoring request
        /// </summary>
        /// <param name="req">The request</param>
        /// <returns>Whether the request should be processed</returns>
        bool HandleRequest(HttpListenerRequest req);

        /// <summary>
        ///     Should start the security monitoring
        /// </summary>
        void Start();

        /// <summary>
        ///     Should stop the security monitoring, but it must be able to be started again
        /// </summary>
        void Stop();
    }
}