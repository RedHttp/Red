namespace RHttpServer.Default
{
    /// <summary>
    ///     The default security settings
    /// </summary>
    public class SimpleHttpSecuritySettings : RPlugin, IHttpSecuritySettings
    {
        /// <summary>
        ///     Default settings for security
        /// </summary>
        /// <param name="sessLenSec"></param>
        /// <param name="maxReqsPrSess"></param>
        /// <param name="banTimeMin"></param>
        public SimpleHttpSecuritySettings(int sessLenSec = 600, int maxReqsPrSess = 1000, int banTimeMin = 60)
        {
            SessionLengthSeconds = sessLenSec;
            MaxRequestsPerSession = maxReqsPrSess;
            BanTimeMinutes = banTimeMin;
        }

        public int SessionLengthSeconds { get; set; }
        public int MaxRequestsPerSession { get; set; }
        public int BanTimeMinutes { get; set; }
    }
}