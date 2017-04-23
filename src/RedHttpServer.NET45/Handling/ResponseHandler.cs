using System.IO;
using System.Net;
using RedHttpServerNet45.Response;

namespace RedHttpServerNet45.Handling
{
    internal abstract class ResponseHandler
    {
        protected static readonly string[] IndexFiles =
        {
            "index.html",
            "index.htm",
            "default.html",
            "default.htm",
        };

        public abstract bool Handle(string route, HttpListenerContext context);

        protected static void GetRange(string range, out int rangeStart, out int rangeEnd)
        {
            var split = range.Split('-');
            if (string.IsNullOrEmpty(split[0]))
                rangeStart = -1;
            else
                int.TryParse(split[0], out rangeStart);
            if (string.IsNullOrEmpty(split[1]))
                rangeEnd = -1;
            else
                int.TryParse(split[1], out rangeEnd);
        }

        protected static string GetType(string input)
        {
            string ret;
            if (!RResponse.MimeTypes.TryGetValue(Path.GetExtension(input), out ret))
                ret = "application/octet-stream";
            return ret;
        }
    }
}