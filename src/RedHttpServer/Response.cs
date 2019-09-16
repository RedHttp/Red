using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Red.Interfaces;

namespace Red
{
    /// <summary>
    ///     Class representing the response to a clients request
    ///     All
    /// </summary>
    public sealed class Response
    {
           

        internal Response(Context context, HttpResponse aspNetResponse)
        {
            Context = context;
            AspNetResponse = aspNetResponse;
        }

        /// <summary>
        ///     Add header item to response
        /// </summary>
        /// <param name="headerName">The name of the header</param>
        /// <param name="headerValue">The value of the header</param>
        public void AddHeader(string headerName, string headerValue)
        {
            AspNetResponse.Headers.Add(headerName, headerValue);
        }

        /// <summary>
        ///     The Red.Context the response is part of
        /// </summary>
        public readonly Context Context;

        /// <summary>
        ///     The ASP.NET HttpResponse that is wrapped
        /// </summary>
        public readonly HttpResponse AspNetResponse;
        
        /// <summary>
        ///     Redirects the client to a given path or url
        /// </summary>
        /// <param name="redirectPath">The path or url to redirect to</param>
        /// <param name="permanent">Whether to respond with a temporary or permanent redirect</param>
        public Task<HandlerType> Redirect(string redirectPath, bool permanent = false)
        {
            Context.Response.Redirect(redirectPath, permanent);
            return Utils.CachedFinalHandlerTask;
        }

        /// <summary>
        ///     Sends data as text
        /// </summary>
        /// <param name="data">The text data to send</param>
        /// <param name="contentType">The mime type of the content</param>
        /// <param name="fileName">If the data represents a file, the filename can be set through this</param>
        /// <param name="attachment">Whether the file should be sent as attachment or inline</param>
        /// <param name="status">The status code for the response</param>
        public Task<HandlerType> SendString(string data, string contentType = "text/plain", string fileName = "",
            bool attachment = false, HttpStatusCode status = HttpStatusCode.OK)
        {
            return SendString(AspNetResponse, data, contentType, fileName, attachment, status);
        }

        /// <summary>
        ///     Static helper for use in middleware
        /// </summary>
        public static async Task<HandlerType> SendString(HttpResponse response, string data, string contentType,
            string fileName, bool attachment, HttpStatusCode status)
        {
            response.StatusCode = (int) status;
            response.ContentType = contentType;
            if (!string.IsNullOrEmpty(fileName))
            {
                var contentDisposition = $"{(attachment ? "attachment" : "inline")}; filename=\"{fileName}\"";
                response.Headers.Add("Content-disposition", contentDisposition);
                
            }
            await response.WriteAsync(data);
            return HandlerType.Final;
        }

        /// <summary>
        ///     Send a empty response with a status code
        /// </summary>
        /// <param name="response">The HttpResponse object</param>
        /// <param name="status">The status code for the response</param>
        public static Task<HandlerType> SendStatus(HttpResponse response, HttpStatusCode status)
        {
            return SendString(response, status.ToString(), "text/plain", "", false, status);
        }
        /// <summary>
        ///     Send a empty response with a status code
        /// </summary>
        /// <param name="status">The status code for the response</param>
        public Task<HandlerType> SendStatus(HttpStatusCode status)
        {
            return SendString(status.ToString(), status: status);
        }

        /// <summary>
        ///     Sends object serialized to text using the current IJsonConverter plugin
        /// </summary>
        /// <param name="data">The object to be serialized and send</param>
        /// <param name="status">The status code for the response</param>
        public Task<HandlerType> SendJson(object data, HttpStatusCode status = HttpStatusCode.OK)
        {
            var json = Context.Plugins.Get<IJsonConverter>().Serialize(data);
            return SendString(json, "application/json", status: status);
        }

        /// <summary>
        ///     Sends object serialized to text using the current IXmlConverter plugin
        /// </summary>
        /// <param name="data">The object to be serialized and send</param>
        /// <param name="status">The status code for the response</param>
        public Task<HandlerType> SendXml(object data, HttpStatusCode status = HttpStatusCode.OK)
        {
            var xml = Context.Plugins.Get<IXmlConverter>().Serialize(data);
            return SendString(xml, "application/xml", status: status);
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
        public async Task<HandlerType> SendStream(Stream dataStream, string contentType, string fileName = "",
            bool attachment = false, bool dispose = true, HttpStatusCode status = HttpStatusCode.OK)
        {
            AspNetResponse.StatusCode = (int) status;
            AspNetResponse.ContentType = contentType;
            if (!string.IsNullOrEmpty(fileName))
            {
                AddHeader("Content-disposition", $"{(attachment ? "attachment" : "inline")}; filename=\"{fileName}\"");
            }
            
            await dataStream.CopyToAsync(AspNetResponse.Body);
            if (dispose)
            {
                dataStream.Dispose();
            }
            
            return HandlerType.Final;
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
        /// <param name="fileName">Filename to show in header, instead of actual filename</param>
        /// <param name="status">The status code for the response</param>
        public async Task<HandlerType> SendFile(string filePath, string contentType = null, bool handleRanges = true, 
            string fileName = null, HttpStatusCode status = HttpStatusCode.OK)
        {
            if (handleRanges) AddHeader("Accept-Ranges", "bytes");

            var fileSize = new FileInfo(filePath).Length;
            var range = Context.Request.TypedHeaders.Range;
            var encodedFilename = WebUtility.UrlEncode(fileName ?? Path.GetFileName(filePath));
            
            if (range != null && range.Ranges.Any())
            {
                var firstRange = range.Ranges.First();
                if (range.Unit != "bytes" || !firstRange.From.HasValue && !firstRange.To.HasValue)
                {
                    await SendStatus(HttpStatusCode.BadRequest);
                    return HandlerType.Error;
                }

                var offset = firstRange.From ?? fileSize - firstRange.To.Value;
                var length = firstRange.To.HasValue
                    ? fileSize - offset - (fileSize - firstRange.To.Value)
                    : fileSize - offset;

                AspNetResponse.StatusCode = (int) HttpStatusCode.PartialContent;
                AspNetResponse.ContentType = Utils.GetMimeType(contentType, filePath);
                AspNetResponse.ContentLength = length;
                AddHeader("Content-Disposition", $"inline; filename=\"{encodedFilename}\"");
                AddHeader("Content-Range", $"bytes {offset}-{offset + length - 1}/{fileSize}");
                await AspNetResponse.SendFileAsync(filePath, offset, length);
            }
            else
            {
                AspNetResponse.StatusCode = (int) status;
                AspNetResponse.ContentType = Utils.GetMimeType(contentType, filePath);
                AspNetResponse.ContentLength = fileSize;
                AddHeader("Content-Disposition", $"inline; filename=\"{encodedFilename}\"");
                await AspNetResponse.SendFileAsync(filePath);
            }

            return HandlerType.Final;
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
        public async Task<HandlerType> Download(string filePath, string fileName = null, string contentType = "",
            HttpStatusCode status = HttpStatusCode.OK)
        {
            AspNetResponse.StatusCode = (int) status;
            AspNetResponse.ContentType = Utils.GetMimeType(contentType, filePath);
            var name = string.IsNullOrEmpty(fileName) ? Path.GetFileName(filePath) : fileName;
            AddHeader("Content-disposition", $"attachment; filename=\"{name}\"");
            await AspNetResponse.SendFileAsync(filePath);
            return HandlerType.Final;
        }
    }
}