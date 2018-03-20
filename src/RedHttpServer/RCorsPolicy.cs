using System.Collections.Generic;

namespace Red
{
    /// <summary>
    /// The Cross-Origin Resource Sharing (CORS) policy for the webserver
    /// </summary>
    public class RCorsPolicy
    {
        /// <summary>
        ///     The allowed origins for CORS requests
        /// </summary>
        public List<string> AllowedOrigins { get; } = new List<string>();

        /// <summary>
        ///     The allowed headers for CORS requests
        /// </summary>
        public List<string> AllowedHeaders { get; } = new List<string>();

        /// <summary>
        ///     The allowed http methods for CORS requests
        /// </summary>
        public List<string> AllowedMethods { get; } = new List<string>();
    }
}