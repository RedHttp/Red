using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using HttpMultipartParser;
using RedHttpServer.Plugins;

namespace RedHttpServer.Request
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
        
        private readonly NameValueCollection _emptyNameValueCollection = new NameValueCollection();
        private readonly RPluginCollection _rp;

        private NameValueCollection _postFormData;

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
        ///     Returns form-data from post request, if any
        /// </summary>
        /// <returns></returns>
        public MultipartFormDataParser GetFormData()
        {
            return new MultipartFormDataParser(UnderlyingRequest.InputStream);
        }

        /// <summary>
        ///     Save multipart form-data from request body, if any, to a file in the specified directory
        /// </summary>
        /// <param name="filePath">The directory to placed to file in</param>
        /// <param name="filerenamer">Function to rename the file(s)</param>
        /// <param name="maxSizeKb">The max filesize allowed</param>
        /// <returns>Whether the file was saved succesfully</returns>
        public Task<bool> SaveBodyToFile(string filePath, Func<string, string> filerenamer = null, long maxSizeKb = 1024)
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