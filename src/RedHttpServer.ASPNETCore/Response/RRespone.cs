using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;
using RedHttpServer.Plugins.Interfaces;

namespace RedHttpServer.Response
{
    /// <summary>
    ///     Class representing the reponse to a clients request
    ///     All
    /// </summary>
    public sealed class RRespone
    {
        internal static readonly IDictionary<string, string> MimeTypes =
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

        internal RRespone(HttpResponse resp, RPluginCollection plugins)
        {
            UnderlyingResponse = resp;
            ServerPlugins = plugins;
        }

        /// <summary>
        ///     Add header item to response
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        public void AddHeader(string fieldName, string fieldValue)
        {
            UnderlyingResponse.Headers.Add(fieldName, fieldValue);
        }
        /// <summary>
        ///     The underlying HttpResponse
        ///     <para />
        ///     The implementation of RRequest is leaky, to avoid limiting you
        /// </summary>
        public HttpResponse UnderlyingResponse { get; }

        /// <summary>
        /// The available plugins
        /// </summary>
        public RPluginCollection ServerPlugins { get; set; }

        /// <summary>
        ///     Redirects the client to a given path or url
        /// </summary>
        /// <param name="redirectPath">The path or url to redirect to</param>
        public void Redirect(string redirectPath)
        {
            UnderlyingResponse.Redirect(redirectPath);
        }

        /// <summary>
        ///     Sends data as text
        /// </summary>
        /// <param name="data">The text data to send</param>
        /// <param name="contentType">The mime type of the content</param>
        /// <param name="filename">If the data represents a file, the filename can be set through this</param>
        /// <param name="status">The status code for the response</param>
        public async void SendString(string data, string contentType = "text/plain", string filename = "",
            int status = (int) HttpStatusCode.OK)
        {
            UnderlyingResponse.StatusCode = status;
            UnderlyingResponse.ContentType = contentType;
            if (!string.IsNullOrEmpty(filename))
                UnderlyingResponse.Headers.Add("Content-disposition", $"inline; filename=\"{filename}\"");
            await UnderlyingResponse.WriteAsync(data);
        }

        /// <summary>
        ///     Sends object serialized to text using the current IJsonConverter plugin
        /// </summary>
        /// <param name="data">The object to be serialized and send</param>
        /// <param name="status">The status code for the response</param>
        public async void SendJson(object data, int status = (int) HttpStatusCode.OK)
        {
            UnderlyingResponse.StatusCode = status;
            UnderlyingResponse.ContentType = "application/json";
            await UnderlyingResponse.WriteAsync(ServerPlugins.Use<IJsonConverter>().Serialize(data));
        }

        /// <summary>
        ///     Sends object serialized to text using the current IXmlConverter plugin
        /// </summary>
        /// <param name="data">The object to be serialized and send</param>
        /// <param name="status">The status code for the response</param>
        public async void SendXml(object data, int status = (int) HttpStatusCode.OK)
        {
            UnderlyingResponse.StatusCode = status;
            UnderlyingResponse.ContentType = "application/xml";
            await UnderlyingResponse.WriteAsync(ServerPlugins.Use<IXmlConverter>().Serialize(data));
        }

        /// <summary>
        ///     Sends file as response and requests the data to be displayed in-browser if possible
        /// </summary>
        /// <param name="filepath">The local path of the file to send</param>
        /// <param name="contentType">
        ///     The mime type for the file, when set to null, the system will try to detect based on file
        ///     extension
        /// </param>
        /// <param name="status">The status code for the response</param>
        public async void SendFile(string filepath, string contentType = null, int status = (int) HttpStatusCode.OK)
        {
            UnderlyingResponse.StatusCode = status;
            if (contentType == null && !MimeTypes.TryGetValue(Path.GetExtension(filepath), out contentType))
                contentType = "application/octet-stream";
            UnderlyingResponse.ContentType = contentType;
            UnderlyingResponse.Headers.Add("Accept-Ranges", "bytes");
            UnderlyingResponse.Headers.Add("Content-disposition",
                "inline; filename=\"" + Path.GetFileName(filepath) + "\"");
            await UnderlyingResponse.SendFileAsync(filepath);
        }

        /// <summary>
        ///     Sends file as response and requests the data to be displayed in-browser if possible
        /// </summary>
        /// <param name="filepath">The local path of the file to send</param>
        /// <param name="rangeStart">The offset in the file</param>
        /// <param name="rangeEnd">The position of the last byte to send, in the file</param>
        /// <param name="contentType">
        ///     The mime type for the file, when set to null, the system will try to detect based on file
        ///     extension
        /// </param>
        /// <param name="status">The status code for the response</param>
        public async void SendFile(string filepath, long rangeStart, long rangeEnd, string contentType = "",
            int status = (int) HttpStatusCode.PartialContent)
        {
            UnderlyingResponse.StatusCode = status;
            if (contentType == null && !MimeTypes.TryGetValue(Path.GetExtension(filepath), out contentType))
                contentType = "application/octet-stream";
            UnderlyingResponse.ContentType = contentType;
            UnderlyingResponse.Headers.Add("Accept-Ranges", "bytes");
            UnderlyingResponse.Headers.Add("Content-disposition",
                "inline; filename=\"" + Path.GetFileName(filepath) + "\"");
            await UnderlyingResponse.SendFileAsync(filepath, rangeStart, rangeEnd);
        }

        /// <summary>
        ///     Sends file as response and requests the data to be downloaded as an attachment
        /// </summary>
        /// <param name="filepath">The local path of the file to send</param>
        /// <param name="filename">The name filename the client receives the file with, defaults to using the actual filename</param>
        /// <param name="contentType">
        ///     The mime type for the file, when set to null, the system will try to detect based on file
        ///     extension
        /// </param>
        /// <param name="status">The status code for the response</param>
        public async void Download(string filepath, string filename = "", string contentType = "",
            int status = (int) HttpStatusCode.OK)
        {
            UnderlyingResponse.StatusCode = status;
            if (contentType == null && !MimeTypes.TryGetValue(Path.GetExtension(filepath), out contentType))
                contentType = "application/octet-stream";
            UnderlyingResponse.ContentType = contentType;
            UnderlyingResponse.Headers.Add("Content-disposition", "attachment; filename=\"" +
                                                                  (string.IsNullOrEmpty(filename)
                                                                      ? Path.GetFileName(filepath)
                                                                      : filename) + "\"");
            await UnderlyingResponse.SendFileAsync(filepath);
        }

        /// <summary>
        ///     Renders a page file using the current IPageRenderer plugin
        /// </summary>
        /// <param name="pagefilepath">The path of the file to be rendered</param>
        /// <param name="parameters">The parameter collection used when replacing data</param>
        /// <param name="status">The status code for the response</param>
        public async void RenderPage(string pagefilepath, RenderParams parameters, int status = (int) HttpStatusCode.OK)
        {
            UnderlyingResponse.StatusCode = status;
            UnderlyingResponse.ContentType = "text/html";
            await UnderlyingResponse.WriteAsync(ServerPlugins.Use<IPageRenderer>().Render(pagefilepath, parameters));
        }
    }
}