using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HttpMultipartParser;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using RedHttpServer.Plugins.Interfaces;
using ServiceStack;

namespace RedHttpServer.Request
{
    /// <summary>
    ///     Class representing a request from a client
    /// </summary>
    public sealed class RRequest
    {
        private static readonly IFormCollection EmptyFormCol = new FormCollection(new Dictionary<string, StringValues>());

        internal RRequest(HttpRequest req, RPluginCollection plugins)
        {
            UnderlyingRequest = req;
            Params = new RequestParams(req.HttpContext);
            ServerPlugins = plugins;
        }

        /// <summary>
        /// The available plugins
        /// </summary>
        public RPluginCollection ServerPlugins { get; set; }

        /// <summary>
        ///     The underlying HttpRequest
        ///     <para />
        ///     The implementation of RRequest is leaky, to avoid limiting you
        /// </summary>
        public HttpRequest UnderlyingRequest { get; }

        /// <summary>
        ///     The url parameters of the request
        /// </summary>
        public RequestParams Params { get; }

        /// <summary>
        ///     The query elements of the request
        /// </summary>
        public IQueryCollection Queries => UnderlyingRequest.Query;

        /// <summary>
        ///     The headers contained in the request
        /// </summary>
        public IHeaderDictionary Headers => UnderlyingRequest.Headers;

        /// <summary>
        ///     The cookies contained in the request
        /// </summary>
        public IRequestCookieCollection Cookies => UnderlyingRequest.Cookies;

        /// <summary>
        ///     Returns the body stream of the request
        ///     and null if the request does not contain a body
        /// </summary>
        /// <returns></returns>
        public Stream GetBodyStream() => UnderlyingRequest.Body;

        /// <summary>
        ///     Returns the body deserialized or parsed to specified type, if possible, default if not
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> ParseBodyAsync<T>() => await ServerPlugins.Use<IBodyParser>().ParseBodyAsync<T>(UnderlyingRequest);

        /// <summary>
        ///     Returns form-data from post request, if any
        /// </summary>
        /// <returns></returns>
        public async Task<IFormCollection> GetFormDataAsync()
        {
            if (!UnderlyingRequest.HasFormContentType)
                return EmptyFormCol;
            return await UnderlyingRequest.ReadFormAsync();
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
            if (UnderlyingRequest.Body == Stream.Null) return Task.FromResult(false);
            var maxbytes = maxSizeKb << 10;
            var tcs = new TaskCompletionSource<bool>();
            var filestreams = new Dictionary<string, Stream>();
            var files = new Dictionary<string, string>();
            var sizes = new Dictionary<string, long>();
            var failed = false;
            var parser = new StreamingMultipartFormDataParser(UnderlyingRequest.Body);
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
                    if (!files.TryGetValue(fname, out string rename))
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
                if (!filestreams.TryGetValue(fname, out Stream stream))
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