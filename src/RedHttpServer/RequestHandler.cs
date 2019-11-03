using System;
using System.Threading.Tasks;

namespace Red
{
    internal class RequestHandler
    {
        internal RequestHandler(string path, string method, Func<Request, Response, Task<HandlerType>>[] handlers)
        {
            Path = path;
            Method = method;
            _handlers = handlers;
        }
        internal readonly string Path;
        internal readonly string Method;
        private readonly Func<Request, Response, Task<HandlerType>>[] _handlers;
        
        internal async Task<HandlerType> Invoke(Request req, Response res)
        {
            var status = HandlerType.Continue;
            foreach (var handler in _handlers)
            {
                status = await handler(req, res);
                if (status != HandlerType.Continue) break;
            }

            return status;
        }

    }
}