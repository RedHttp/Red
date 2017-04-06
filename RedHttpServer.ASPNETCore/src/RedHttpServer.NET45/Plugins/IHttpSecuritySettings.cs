namespace RHttpServer
{
    /// <summary>
    ///     The minimum interface that must be implented in a class for it to be used as security settings
    /// </summary>
    public interface IHttpSecuritySettings
    {
        /// <summary>
        ///     For how long should the amount of requests made per client be counted
        /// </summary>
        int SessionLengthSeconds { get; set; }

        /// <summary>
        ///     The maximum amount of requests allowed per client in the period of one session
        /// </summary>
        int MaxRequestsPerSession { get; set; }

        /// <summary>
        ///     Amount of minutes to ban a client when exceeding the maximum requests per session
        /// </summary>
        int BanTimeMinutes { get; set; }
    }
}