using System.Net;
using System.Threading.Tasks;
using Red.Extensions;

namespace Red
{
    /// <summary>
    /// Utilities
    /// </summary>
    public static partial class Utils
    {
        /// <summary>
        /// Parsing middleware.
        /// Attempts to parse the body using ParseBodyAsync.
        /// If unable to parse the body, responds with Bad Request status.
        /// Otherwise saves the parsed object using SetData on the request, so it can be retrieved using GetData by a later handler.
        /// </summary>
        /// <param name="req">The request object</param>
        /// <param name="res">The response object</param>
        /// <typeparam name="T">The type to parse the body to</typeparam>
        /// <returns></returns>
        public static async Task<Response.Type> CanParse<T>(Request req, Response res)
            where T : class
        {
            var obj = await req.ParseBodyAsync<T>();
            if (obj == default)
            {
                await res.SendStatus(HttpStatusCode.BadRequest);
                return Response.Type.Error;
            }
            else
            {
                req.SetData(obj);
                return Response.Type.Continue;
            }
        }

        /// <summary>
        /// Parsing middleware.
        /// Attempts to parse the body using ParseBodyAsync.
        /// If unable to parse the body, responds with Bad Request status.
        /// Otherwise saves the parsed object using SetData on the request, so it can be retrieved using GetData by a later handler.
        /// </summary>
        /// <param name="req">The request object</param>
        /// <param name="res">The response object</param>
        /// <param name="wsd">The websocket dialog (not modified)</param>
        /// <typeparam name="T">The type to parse the body to</typeparam>
        /// <returns></returns>
        public static Task CanParse<T>(Request req, Response res, WebSocketDialog wsd)
            where T : class => CanParse<T>(req, res);
    }
}