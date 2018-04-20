using System;
using System.Threading.Tasks;
using Red;

namespace JwtSessions
{
    public static class JwtSessionExtensions
    {
        /// <summary>
        ///     Creates a Jwt token and sends it wrapped in an object: { "JWT": "Bearer eyJ0eXAiOiJKV1QiL..." }
        /// </summary>
        /// <param name="response"></param>
        /// <param name="sessionData"></param>
        public static async Task SendJwtToken<TSession>(this Response response, TSession sessionData)
        {
            var manager = response.ServerPlugins.Get<JwtSessions<TSession>>();
            var auth = manager.NewSession(sessionData);
            await response.SendJson(new { JWT = auth });
        }
    }
}