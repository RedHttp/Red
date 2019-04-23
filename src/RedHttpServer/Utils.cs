using System;
using System.Collections.Generic;
using System.IO;
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
        public static async Task<HandlerType> CanParse<T>(Request req, Response res)
            where T : class
        {
            var obj = await req.ParseBodyAsync<T>();
            if (obj == default)
            {
                await res.SendStatus(HttpStatusCode.BadRequest);
                return HandlerType.Error;
            }
            else
            {
                req.SetData(obj);
                return HandlerType.Continue;
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


        /// <summary>
        /// Middleware for serving static files
        /// </summary>
        /// <param name="basePath">The path of the base directory the files are served from</param>
        /// <param name="send404NotFound">Whether to respond with 404 when file not found, or to continue to next handler</param>
        /// <returns></returns>
        public static Func<Request, Response, Task<HandlerType>> SendFiles(string basePath, bool send404NotFound = true)
        {
            var fullBasePath = Path.GetFullPath(basePath);
            return (req, res) =>
            {
                var relativeFilepath = req.Context.ExtractUrlParameter("any");
                var absoluteFilePath = Path.Combine(fullBasePath, relativeFilepath);
                if (!absoluteFilePath.StartsWith(fullBasePath) || !File.Exists(absoluteFilePath))
                {
                    return send404NotFound 
                        ? res.SendStatus(HttpStatusCode.NotFound) 
                        : Task.FromResult(HandlerType.Continue);
                }
                else
                {
                    return res.SendFile(absoluteFilePath);
                }
            };
        }



        internal static string GetMimeType(string contentType, string filePath,
            string defaultContentType = "application/octet-stream")
        {
            if (string.IsNullOrEmpty(contentType) && !Utils.MimeTypes.TryGetValue(Path.GetExtension(filePath), out contentType))
            {
                contentType = defaultContentType;
            }
            return contentType;
        }

        private static readonly IDictionary<string, string> MimeTypes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                #region extension to MIME type list

                {".asf", "video/x-ms-asf"},
                {".asx", "video/x-ms-asf"},
                {".avi", "video/x-msvideo"},
                {".bin", "application/octet-stream"},
                {".cco", "application/x-cocoa"},
                {".crt", "application/x-x509-ca-cert"},
                {".css", "text/css"},
                {".deb", "application/octet-stream"},
                {".der", "application/x-x509-ca-cert"},
                {".dll", "application/octet-stream"},
                {".dmg", "application/octet-stream"},
                {".ear", "application/java-archive"},
                {".eot", "application/octet-stream"},
                {".exe", "application/octet-stream"},
                {".flv", "video/x-flv"},
                {".gif", "image/gif"},
                {".hqx", "application/mac-binhex40"},
                {".htc", "text/x-component"},
                {".htm", "text/html"},
                {".html", "text/html"},
                {".ico", "image/x-icon"},
                {".img", "application/octet-stream"},
                {".iso", "application/octet-stream"},
                {".jar", "application/java-archive"},
                {".jardiff", "application/x-java-archive-diff"},
                {".jng", "image/x-jng"},
                {".jnlp", "application/x-java-jnlp-file"},
                {".jpeg", "image/jpeg"},
                {".jpg", "image/jpeg"},
                {".js", "application/x-javascript"},
                {".json", "text/json"},
                {".mml", "text/mathml"},
                {".mng", "video/x-mng"},
                {".mov", "video/quicktime"},
                {".mp3", "audio/mpeg"},
                {".mp4", "video/mp4"},
                {".mpeg", "video/mpeg"},
                {".mpg", "video/mpeg"},
                {".msi", "application/octet-stream"},
                {".msm", "application/octet-stream"},
                {".msp", "application/octet-stream"},
                {".pdb", "application/x-pilot"},
                {".pdf", "application/pdf"},
                {".pem", "application/x-x509-ca-cert"},
                {".php", "text/x-php"},
                {".pl", "application/x-perl"},
                {".pm", "application/x-perl"},
                {".png", "image/png"},
                {".prc", "application/x-pilot"},
                {".ra", "audio/x-realaudio"},
                {".rar", "application/x-rar-compressed"},
                {".rpm", "application/x-redhat-package-manager"},
                {".rss", "text/xml"},
                {".run", "application/x-makeself"},
                {".sea", "application/x-sea"},
                {".shtml", "text/html"},
                {".sit", "application/x-stuffit"},
                {".swf", "application/x-shockwave-flash"},
                {".tcl", "application/x-tcl"},
                {".tk", "application/x-tcl"},
                {".txt", "text/plain"},
                {".war", "application/java-archive"},
                {".webm", "video/webm"},
                {".wbmp", "image/vnd.wap.wbmp"},
                {".wmv", "video/x-ms-wmv"},
                {".xml", "text/xml"},
                {".xpi", "application/x-xpinstall"},
                {".zip", "application/zip"}

                #endregion
            };

    }
}