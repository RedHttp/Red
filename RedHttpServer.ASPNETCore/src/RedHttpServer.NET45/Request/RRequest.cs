using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using HttpMultipartParser;

namespace RHttpServer
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
            _cookies = new Lazy<RCookies>(() => new RCookies(UnderlyingRequest.Cookies));
            _headers = new Lazy<RHeaders>(() => new RHeaders(UnderlyingRequest.Headers));
            _queries = new Lazy<RQueries>(() => new RQueries(UnderlyingRequest.QueryString));
        }

        internal RRequest()
        {
        }

        private readonly Lazy<RCookies> _cookies;
        private readonly NameValueCollection _emptyNameValueCollection = new NameValueCollection();
        private readonly Lazy<RHeaders> _headers;
        private readonly Lazy<RQueries> _queries;

        private readonly RPluginCollection _rp;

        private NameValueCollection _postFormData;

        /// <summary>
        ///     The query elements of the request
        /// </summary>
        public RQueries Queries => _queries.Value;

        /// <summary>
        ///     The headers contained in the request
        /// </summary>
        public RHeaders Headers => _headers.Value;

        /// <summary>
        ///     The cookies contained in the request
        /// </summary>
        public RCookies Cookies => _cookies.Value;

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
        public Stream GetBodyStream()
        {
            if (!UnderlyingRequest.HasEntityBody || UnderlyingRequest.InputStream == Stream.Null) return null;
            return UnderlyingRequest.InputStream;
        }

        /// <summary>
        ///     Returns the body deserialized or parsed to specified type, if possible, default if not
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ParseBody<T>()
        {
            return _rp.Use<IBodyParser>().ParseBody<T>(UnderlyingRequest);
        }

        /// <summary>
        ///     Returns the body deserialized or parsed to specified type, if possible, default if not
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> ParseBodyAsync<T>()
        {
            return await _rp.Use<IBodyParser>().ParseBodyAsync<T>(UnderlyingRequest);
        }

        /// <summary>
        ///     Returns form-data from post request, if any
        /// </summary>
        /// <returns></returns>
        public NameValueCollection GetBodyPostFormData()
        {
            if (_postFormData != null) return _postFormData;
            if (!UnderlyingRequest.ContentType.Contains("x-www-form-urlencoded"))
            {
                _postFormData = _emptyNameValueCollection;
                return _emptyNameValueCollection;
            }
            using (var reader = new StreamReader(UnderlyingRequest.InputStream))
            {
                var txt = reader.ReadToEnd();
                _postFormData = HttpUtility.ParseQueryString(txt);
                return _postFormData;
            }
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