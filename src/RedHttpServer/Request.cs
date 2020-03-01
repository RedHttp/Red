using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;

namespace Red
{
    /// <summary>
    ///     Class representing a request from a client
    /// </summary>
    public sealed class Request : InContext
    {
        private readonly Lazy<RequestHeaders> _typedHeaders;

        /// <summary>
        ///     The ASP.NET HttpRequest that is wrapped
        /// </summary>
        public readonly HttpRequest AspNetRequest;

        private IFormCollection? _form;

        internal Request(Context context) : base(context)
        {
            AspNetRequest = context.AspNetContext.Request;
            _typedHeaders = new Lazy<RequestHeaders>(AspNetRequest.GetTypedHeaders);
        }

        /// <summary>
        ///     The query elements of the request
        /// </summary>
        public IQueryCollection Queries => AspNetRequest.Query;

        /// <summary>
        ///     The cancellation token the request being aborted
        /// </summary>
        public CancellationToken Aborted => AspNetRequest.HttpContext.RequestAborted;

        /// <summary>
        ///     The headers contained in the request
        /// </summary>
        public IHeaderDictionary Headers => AspNetRequest.Headers;
        
        /// <summary>
        ///     The headers contained in the request
        /// </summary>
        public UrlParameters Params => Context.Params;


        /// <summary>
        ///     Exposes the typed headers for the request
        /// </summary>
        public RequestHeaders TypedHeaders => _typedHeaders.Value;

        /// <summary>
        ///     The cookies contained in the request
        /// </summary>
        public IRequestCookieCollection Cookies => AspNetRequest.Cookies;

        /// <summary>
        ///     Returns the body stream of the request
        /// </summary>
        public Stream BodyStream => AspNetRequest.Body;

        /// <summary>
        ///     Returns form-data from request, if any, null otherwise.
        /// </summary>
        public async Task<IFormCollection?> GetFormDataAsync()
        {
            if (!AspNetRequest.HasFormContentType)
                return null;

            if (_form != null)
                return _form;

            _form = await AspNetRequest.ReadFormAsync(Aborted);
            return _form;
        }

        /// <summary>
        ///     Save all files in requests to specified directory.
        /// </summary>
        /// <param name="saveDir">The directory to place the file(s) in</param>
        /// <param name="fileRenamer">Function to rename the file(s)</param>
        /// <param name="maxSizeKb">The max total filesize allowed</param>
        /// <returns>Whether the file(s) was saved successfully</returns>
        public async Task<bool> SaveFiles(string saveDir, Func<string, string>? fileRenamer = null,
            long maxSizeKb = 50000)
        {
            if (!AspNetRequest.HasFormContentType) return false;
            var form = await AspNetRequest.ReadFormAsync(Aborted);
            if (form.Files.Sum(file => file.Length) > maxSizeKb << 10)
                return false;

            var fullSaveDir = Path.GetFullPath(saveDir);
            foreach (var formFile in form.Files)
            {
                var filename = fileRenamer == null ? formFile.FileName : fileRenamer(formFile.FileName);
                filename = Path.GetFileName(filename);
                if (string.IsNullOrWhiteSpace(filename)) continue;
                
                var filepath = Path.Combine(fullSaveDir, filename);
                await using var fileStream = File.Create(filepath);
                await formFile.CopyToAsync(fileStream, Aborted);
            }

            return true;
        }
    }
}