using System.IO;
using System.Linq;
using System.Net;

namespace RHttpServer
{
    internal sealed class CachePublicFileRequestHander : ResponseHandler
    {
        public CachePublicFileRequestHander(string publicDir, IFileCacheManager cache)
        {
            _pdir = publicDir;
            _cacheMan = cache;
        }

        private readonly IFileCacheManager _cacheMan;
        private readonly string _pdir;

        public override bool Handle(string route, HttpListenerContext context)
        {
            var range = context.Request.Headers["Range"];
            var rangeSet = false;
            int rangeStart = 0, rangeEnd = 0;
            if (!string.IsNullOrEmpty(range))
            {
                range = range.Replace("bytes=", "");
                GetRange(range, out rangeStart, out rangeEnd);
                rangeSet = true;
            }

            var publicFile = Path.Combine(_pdir, route);
            // Verify that the file is indeed supposed to be public
            if (!Path.GetFullPath(publicFile).Contains(_pdir))
                return false;

            byte[] temp = null;
            if (_cacheMan.TryGetFile(publicFile, out temp))
            {
                if (rangeSet)
                    new RResponse(context.Response).SendBytes(temp, rangeStart, rangeEnd,
                        GetType(publicFile), publicFile);
                else
                    new RResponse(context.Response).SendBytes(temp, GetType(publicFile), publicFile);
                return true;
            }

            if (File.Exists(publicFile))
            {
                if (rangeSet)
                    new RResponse(context.Response).SendFile(publicFile, rangeStart, rangeEnd);
                else
                    new RResponse(context.Response).SendFile(publicFile);
                _cacheMan.TryAddFile(publicFile);
                return true;
            }

            var pfiles = IndexFiles.Select(x => Path.Combine(publicFile, x)).ToList();
            if (
                !string.IsNullOrEmpty(publicFile = pfiles.FirstOrDefault(iFile => _cacheMan.TryGetFile(iFile, out temp))))
            {
                if (rangeSet)
                    new RResponse(context.Response).SendBytes(temp, rangeStart, rangeEnd,
                        GetType(publicFile), publicFile);
                else
                    new RResponse(context.Response).SendBytes(temp, GetType(publicFile), publicFile);
                return true;
            }
            if (string.IsNullOrEmpty(publicFile = pfiles.FirstOrDefault(File.Exists))) return false;
            if (rangeSet)
                new RResponse(context.Response).SendFile(publicFile, rangeStart, rangeEnd);
            else
                new RResponse(context.Response).SendFile(publicFile);
            _cacheMan.TryAddFile(publicFile);
            return true;
        }
    }
}