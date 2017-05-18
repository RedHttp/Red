using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HttpMultipartParser;
using RedHttpServerNet45.Plugins.Interfaces;
using System.Text.RegularExpressions;

namespace RedHttpServerNet45.Request
{
    /// <summary>
    ///     Class representing a request from a client
    /// </summary>
    public class RRequest
    {
        internal RRequest(HttpListenerRequest req, RequestParams par, RPluginCollection pluginCollection)
        {
            UnderlyingRequest = req;
            Params = par;
            _rp = pluginCollection;
        }

        private readonly RPluginCollection _rp;
        private RFormCollection _postRFormData;
        private static readonly RFormCollection EmptyNameValueCollection = new RFormCollection();

        /// <summary>
        ///     The query elements of the request
        /// </summary>
        public NameValueCollection Queries => UnderlyingRequest.QueryString;

        /// <summary>
        ///     The headers contained in the request
        /// </summary>
        public NameValueCollection Headers => UnderlyingRequest.Headers;

        /// <summary>
        ///     The cookies contained in the request
        /// </summary>
        public CookieCollection Cookies => UnderlyingRequest.Cookies;

        /// <summary>
        ///     The url parameters of the request
        /// </summary>
        public RequestParams Params { get; }

        /// <summary>
        ///     The underlying HttpListenerRequest
        ///     <para />
        ///     The implementation of RRequest is leaky, to avoid limiting you
        /// </summary>
        public HttpListenerRequest UnderlyingRequest { get; }


        /// <summary>
        ///     Returns the body stream of the request
        ///     and null if the request does not contain a body
        /// </summary>
        /// <returns></returns>
        public Stream GetBodyStream() => UnderlyingRequest.InputStream;

        /// <summary>
        ///     Returns the body deserialized or parsed to specified type, if possible, default if not
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> ParseBodyAsync<T>() => await _rp.Use<IBodyParser>().ParseBodyAsync<T>(UnderlyingRequest);

        /// <summary>
        ///     Returns form-data from post request, if any.
        /// </summary>
        /// <returns></returns>
        public async Task<RFormCollection> GetFormDataAsync()
        {
            if (_postRFormData != null) return _postRFormData;
            var ctype = UnderlyingRequest.ContentType;
            if (ctype.Contains("x-www-form-urlencoded"))
                return await FormDataParser.ParseUrlEncoded(UnderlyingRequest.InputStream);
            if (ctype.Contains("multipart/form-data"))
            {
                var m = _webFormBoundaryRegex.Match(ctype);
                if (m.Success)
                {
                    var boundary = m.Groups[1].Value;
                    return await FormDataParser.ParseMultipart(UnderlyingRequest.InputStream, boundary);
                }
            }
            _postRFormData = EmptyNameValueCollection;
            return _postRFormData;
        }

        private static readonly Regex _webFormBoundaryRegex= new Regex("boundary=([A-Za-z0-9-]+);?", RegexOptions.Compiled);

        /// <summary>
        ///     Save multipart form-data from request body, if any, to a file in the specified directory
        /// </summary>
        /// <param name="filePath">The directory to placed to file in</param>
        /// <param name="filerenamer">Function to rename the file(s)</param>
        /// <param name="maxSizeKb">The max filesize allowed. Default is 1mb</param>
        /// <returns>Whether the file was saved succesfully</returns>
        public Task<bool> SaveBodyToFile(string filePath, Func<string, string> filerenamer = null,
            long maxSizeKb = 1024)
        {
            if (!UnderlyingRequest.HasEntityBody) return Task.FromResult(false);
            var maxbytes = maxSizeKb << 10;
            var tcs = new TaskCompletionSource<bool>();
            var filestreams = new Dictionary<string, Stream>();
            var files = new Dictionary<string, string>();
            var sizes = new Dictionary<string, long>();
            var failed = false;
            var parser = new StreamingMultipartFormDataParser(UnderlyingRequest.InputStream);
            parser.FileHandler += (name, fname, type, disposition, buffer, bytes) =>
            {
                if (failed)
                    return;
                if (bytes > maxbytes)
                {
                    tcs.TrySetResult(false);
                    return;
                }
                if (filerenamer != null)
                {
                    string rename;
                    if (!files.TryGetValue(fname, out rename))
                    {
                        rename = filerenamer(fname);
                        files.Add(fname, rename);
                    }
                    fname = rename;
                }
                if (!sizes.ContainsKey(fname))
                    sizes[fname] = bytes;
                else
                    sizes[fname] += bytes;
                if (sizes[fname] > maxbytes)
                {
                    failed = true;
                    foreach (var str in filestreams.Values)
                        str.Close();
                    tcs.TrySetResult(false);
                    return;
                }
                Stream stream;
                if (!filestreams.TryGetValue(fname, out stream))
                {
                    stream = File.Create(Path.Combine(filePath, fname));
                    filestreams.Add(fname, stream);
                }
                stream.Write(buffer, 0, bytes);
                stream.Flush();
            };
            parser.StreamClosedHandler += () =>
            {
                foreach (var stream in filestreams.Values)
                    stream.Close();
                tcs.TrySetResult(true);
            };
            parser.Run();
            return tcs.Task;
        }
    }
}