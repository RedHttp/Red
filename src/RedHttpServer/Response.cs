using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
                {".wbmp", "image/vnd.wap.wbmp"},
                {".wmv", "video/x-ms-wmv"},
                {".xml", "text/xml"},
                {".xpi", "application/x-xpinstall"},
                {".zip", "application/zip"}

                #endregion
            };

        internal Response(HttpResponse resp, PluginCollection plugins)
        {
            UnderlyingResponse = resp;
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
        public HttpResponse UnderlyingResponse { get; }

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
        public Task Redirect(string redirectPath)
        {
            UnderlyingResponse.Redirect(redirectPath);
            return CompletedTask;
        }

        /// <summary>
        ///     Sends data as text
        /// </summary>
        /// <param name="data">The text data to send</param>
        /// <param name="contentType">The mime type of the content</param>
        /// <param name="fileName">If the data represents a file, the filename can be set through this</param>
        /// <param name="status">The status code for the response</param>
        public async Task SendString(string data, string contentType = "text/plain", string fileName = "",
            HttpStatusCode status = HttpStatusCode.OK)
        {
            await SendString(UnderlyingResponse, data, contentType, fileName, status);
            Closed = true;
        }
        /// <summary>
        ///     Static helper for use in middleware
        /// </summary>
        public static async Task SendString(HttpResponse response, string data, string contentType = "text/plain", string fileName = "",
            HttpStatusCode status = HttpStatusCode.OK)
        {
            response.StatusCode = (int) status;
            response.ContentType = contentType;
            if (!string.IsNullOrEmpty(fileName))
                response.Headers.Add("Content-disposition", $"inline; filename=\"{fileName}\"");
            await response.WriteAsync(data);
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
        ///     Static helper for use in middleware
        /// </summary>
        public static async Task SendStatus(HttpResponse response, HttpStatusCode status)
        {
            await SendString(response, status.ToString(), status: status);
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
        /// <param name="dispose">Whether to call dispose on stream when done sending</param>
        /// <param name="status">The status code for the response</param>
        public async Task SendStream(Stream dataStream, string contentType, string fileName = "", bool dispose = true, HttpStatusCode status = HttpStatusCode.OK)
        {
            UnderlyingResponse.StatusCode = (int)status;
            UnderlyingResponse.ContentType = contentType;
            if (fileName != "")
                UnderlyingResponse.Headers.Add("Content-disposition", $"inline; filename=\"{fileName}\"");
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
        /// <param name="status">The status code for the response</param>
        public async Task SendFile(string filePath, string contentType = null, HttpStatusCode status = HttpStatusCode.OK)
        {
            UnderlyingResponse.StatusCode = (int)status;
            if (contentType == null && !MimeTypes.TryGetValue(Path.GetExtension(filePath), out contentType))
                contentType = "application/octet-stream";
            UnderlyingResponse.ContentType = contentType;
            UnderlyingResponse.Headers.Add("Accept-Ranges", "bytes");
            UnderlyingResponse.Headers.Add("Content-disposition", $"inline; filename=\"{Path.GetFileName(filePath)}\"");
            await UnderlyingResponse.SendFileAsync(filePath);
            Closed = true;
        }

        /// <summary>
        ///     Sends file as response and requests the data to be displayed in-browser if possible
        /// </summary>
        /// <param name="filePath">The local path of the file to send</param>
        /// <param name="rangeStart">The offset in the file</param>
        /// <param name="rangeEnd">The position of the last byte to send, in the file</param>
        /// <param name="contentType">
        ///     The mime type for the file, when set to null, the system will try to detect based on file
        ///     extension
        /// </param>
        /// <param name="status">The status code for the response</param>
        public async Task SendFile(string filePath, long rangeStart, long rangeEnd, string contentType = "",
            HttpStatusCode status = HttpStatusCode.PartialContent)
        {
            UnderlyingResponse.StatusCode = (int)status;
            if (contentType == null && !MimeTypes.TryGetValue(Path.GetExtension(filePath), out contentType))
                contentType = "application/octet-stream";
            UnderlyingResponse.ContentType = contentType;
            UnderlyingResponse.Headers.Add("Accept-Ranges", "bytes");
            UnderlyingResponse.Headers.Add("Content-disposition",
                "inline; filename=\"" + Path.GetFileName(filePath) + "\"");
            await UnderlyingResponse.SendFileAsync(filePath, rangeStart, rangeEnd);
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
            if (contentType == null && !MimeTypes.TryGetValue(Path.GetExtension(filePath), out contentType))
                contentType = "application/octet-stream";
            UnderlyingResponse.ContentType = contentType;
            var name = string.IsNullOrEmpty(fileName) ? Path.GetFileName(filePath) : fileName;
            UnderlyingResponse.Headers.Add("Content-disposition", $"attachment; filename=\"{name}\"");
            await UnderlyingResponse.SendFileAsync(filePath);
            Closed = true;
        }

    }
}