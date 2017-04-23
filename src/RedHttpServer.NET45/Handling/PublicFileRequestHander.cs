using System.IO;
using System.Linq;
using System.Net;
using RedHttpServerNet45.Response;

namespace RedHttpServerNet45.Handling
{
    internal sealed class PublicFileRequestHander : ResponseHandler
    {
        public PublicFileRequestHander(string publicDir)
        {
            _pdir = publicDir;
        }

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

            if (File.Exists(publicFile))
            {
                if (!rangeSet)
                    new RResponse(context.Response).SendFile(publicFile);
                else
                    new RResponse(context.Response).SendFile(publicFile, rangeStart, rangeEnd);
                return true;
            }

            var pfiles = IndexFiles.Select(x => Path.Combine(publicFile, x));
            if (string.IsNullOrEmpty(publicFile = pfiles.FirstOrDefault(File.Exists))) return false;
            if (!rangeSet)
                new RResponse(context.Response).SendFile(publicFile);
            else
                new RResponse(context.Response).SendFile(publicFile, rangeStart, rangeEnd);
            return true;
        }
    }
}