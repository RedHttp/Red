using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Red.Interfaces;

namespace Red
{
    /// <summary>
    ///     Class representing the reponse to a clients request
    ///     All
    /// </summary>
    public sealed class Response
    {
        private static readonly Task CompletedTask = Task.CompletedTask;

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

        internal Response(HttpContext context, PluginCollection plugins)
        {
            UnderlyingContext = context;
            ServerPlugins = plugins;
        }

        /// <summary>
        ///     Add header item to response
        /// </summary>
        /// <param name="headerName">The name of the header</param>
        /// <param name="headerValue">The value of the header</param>
        public void AddHeader(string headerName, string headerValue)
        {
            if (Closed) throw new RedHttpServerException("A response has already been sent.");
            UnderlyingResponse.Headers.Add(headerName, headerValue);
        }

        /// <summary>
        ///     The underlying HttpResponse
        ///     <para />
        ///     The implementation of Request is leaky, to avoid limiting you
        /// </summary>
        public HttpResponse UnderlyingResponse => UnderlyingContext.Response;

        /// <summary>
        ///     The underlying HttpContext
        ///     <para />
        ///     The implementation of Request is leaky, to avoid limiting you
        /// </summary>
        public HttpContext UnderlyingContext { get; set; }

        /// <summary>
        ///     The available plugins registered to the server
        /// </summary>
        public PluginCollection ServerPlugins { get; }

        /// <summary>
        ///     Bool indicating whether the response has been sent
        /// </summary>
        public bool Closed { get; set; }

        /// <summary>
        ///     Redirects the client to a given path or url
        /// </summary>
        /// <param name="redirectPath">The path or url to redirect to</param>
        /// <param name="permanent">Whether to respond with a temporary or permanent redirect</param>
        public Task Redirect(string redirectPath, bool permanent = false)
        {
            UnderlyingResponse.Redirect(redirectPath, permanent);
            return CompletedRedirectTask;
        }
        // Cached completed task for Redirect member function
        private static readonly Task CompletedRedirectTask = Task.FromResult(true);

        /// <summary>
        ///     Sends data as text
        /// </summary>
        /// <param name="data">The text data to send</param>
        /// <param name="contentType">The mime type of the content</param>
        /// <param name="fileName">If the data represents a file, the filename can be set through this</param>
        /// <param name="attachment">Whether the file should be sent as attachment or inline</param>
        /// <param name="status">The status code for the response</param>
        public async Task SendString(string data, string contentType = "text/plain", string fileName = "",
            bool attachment = false, HttpStatusCode status = HttpStatusCode.OK)
        {
            await SendString(UnderlyingResponse, data, contentType, fileName, attachment, status);
            Closed = true;
        }

        /// <summary>
        ///     Static helper for use in middleware
        /// </summary>
        public static async Task SendString(HttpResponse response, string data, string contentType = "text/plain",
            string fileName = "",
            bool attachment = false, HttpStatusCode status = HttpStatusCode.OK)
        {
            response.StatusCode = (int) status;
            response.ContentType = contentType;
            if (!string.IsNullOrEmpty(fileName))
                response.Headers.Add("Content-disposition",
                    $"{(attachment ? "attachment" : "inline")}; filename=\"{fileName}\"");
            await response.WriteAsync(data);
        }

        /// <summary>
        ///     Send a empty response with a status code
        /// </summary>
        /// <param name="response">The HttpResponse object</param>
        /// <param name="status">The status code for the response</param>
        public static async Task SendStatus(HttpResponse response, HttpStatusCode status)
        {
            await SendString(response, status.ToString(), status: status);
        }
        /// <summary>
        ///     Send a empty response with a status code
        /// </summary>
        /// <param name="status">The status code for the response</param>
        public async Task SendStatus(HttpStatusCode status)
        {
            await SendString(status.ToString(), status: status);
        }

        /// <summary>
        ///     Sends object serialized to text using the current IJsonConverter plugin
        /// </summary>
        /// <param name="data">The object to be serialized and send</param>
        /// <param name="status">The status code for the response</param>
        public async Task SendJson(object data, HttpStatusCode status = HttpStatusCode.OK)
        {
            var json = ServerPlugins.Get<IJsonConverter>().Serialize(data);
            await SendString(json, "application/json", status: status);
        }

        /// <summary>
        ///     Sends object serialized to text using the current IXmlConverter plugin
        /// </summary>
        /// <param name="data">The object to be serialized and send</param>
        /// <param name="status">The status code for the response</param>
        public async Task SendXml(object data, HttpStatusCode status = HttpStatusCode.OK)
        {
            var xml = ServerPlugins.Get<IXmlConverter>().Serialize(data);
            await SendString(xml, "application/xml", status: status);
        }

        /// <summary>
        ///     Sends all data in stream to response
        /// </summary>
        /// <param name="dataStream">The stream to copy data from</param>
        /// <param name="contentType">The mime type of the data in the stream</param>
        /// <param name="fileName">The filename that the browser should see the data as. Optional</param>
        /// <param name="attachment">Whether the file should be sent as attachment or inline</param>
        /// <param name="dispose">Whether to call dispose on stream when done sending</param>
        /// <param name="status">The status code for the response</param>
        public async Task SendStream(Stream dataStream, string contentType, string fileName = "",
            bool attachment = false, bool dispose = true, HttpStatusCode status = HttpStatusCode.OK)
        {
            UnderlyingResponse.StatusCode = (int) status;
            UnderlyingResponse.ContentType = contentType;
            if (!string.IsNullOrEmpty(fileName))
                AddHeader("Content-disposition", $"{(attachment ? "attachment" : "inline")}; filename=\"{fileName}\"");
            await dataStream.CopyToAsync(UnderlyingResponse.Body);
            if (dispose)
                dataStream.Dispose();
            Closed = true;
        }

        /// <summary>
        ///     Sends file as response and requests the data to be displayed in-browser if possible
        /// </summary>
        /// <param name="filePath">The local path of the file to send</param>
        /// <param name="contentType">
        ///     The mime type for the file, when set to null, the system will try to detect based on file
        ///     extension
        /// </param>
        /// <param name="handleRanges">Whether to enable handling of range-requests for the file(s) served</param>
        /// <param name="status">The status code for the response</param>
        public async Task SendFile(string filePath, string contentType = null, bool handleRanges = true,
            HttpStatusCode status = HttpStatusCode.OK)
        {
            if (handleRanges) AddHeader("Accept-Ranges", "bytes");

            var fileSize = new FileInfo(filePath).Length;
            var range = UnderlyingContext.Request.GetTypedHeaders().Range;

            if (range != null && range.Ranges.Any())
            {
                var firstRange = range.Ranges.First();
                if (range.Unit != "bytes" || (!firstRange.From.HasValue && !firstRange.To.HasValue))
                {
                    await SendStatus(HttpStatusCode.BadRequest);
                    return;
                }

                var offset = firstRange.From ?? fileSize - firstRange.To.Value;
                var length = firstRange.To.HasValue
                    ? fileSize - offset - (fileSize - firstRange.To.Value)
                    : fileSize - offset;

                UnderlyingResponse.StatusCode = (int) HttpStatusCode.PartialContent;
                UnderlyingResponse.ContentType = GetMime(contentType, filePath);
                UnderlyingResponse.ContentLength = length;
                AddHeader("Content-Disposition", $"inline; filename=\"{Path.GetFileName(filePath)}\"");
                AddHeader("Content-Range", $"bytes {offset}-{offset + length - 1}/{fileSize}");
                await UnderlyingResponse.SendFileAsync(filePath, offset, length);
            }
            else
            {
                UnderlyingResponse.StatusCode = (int) status;
                UnderlyingResponse.ContentType = GetMime(contentType, filePath);
                UnderlyingResponse.ContentLength = fileSize;
                AddHeader("Content-Disposition", $"inline; filename=\"{Path.GetFileName(filePath)}\"");
                await UnderlyingResponse.SendFileAsync(filePath);
            }

            Closed = true;
        }

        /// <summary>
        ///     Sends file as response and requests the data to be downloaded as an attachment
        /// </summary>
        /// <param name="filePath">The local path of the file to send</param>
        /// <param name="fileName">The name filename the client receives the file with, defaults to using the actual filename</param>
        /// <param name="contentType">
        ///     The mime type for the file, when set to null, the system will try to detect based on file
        ///     extension
        /// </param>
        /// <param name="status">The status code for the response</param>
        public async Task Download(string filePath, string fileName = null, string contentType = "",
            HttpStatusCode status = HttpStatusCode.OK)
        {
            UnderlyingResponse.StatusCode = (int) status;
            UnderlyingResponse.ContentType = GetMime(contentType, filePath);
            var name = string.IsNullOrEmpty(fileName) ? Path.GetFileName(filePath) : fileName;
            AddHeader("Content-disposition", $"attachment; filename=\"{name}\"");
            await UnderlyingResponse.SendFileAsync(filePath);
            Closed = true;
        }


        private static string GetMime(string contentType, string filePath,
            string defaultContentType = "application/octet-stream")
        {
            if (string.IsNullOrEmpty(contentType) &&
                !MimeTypes.TryGetValue(Path.GetExtension(filePath), out contentType))
                contentType = defaultContentType;
            return contentType;
        }
    }
}