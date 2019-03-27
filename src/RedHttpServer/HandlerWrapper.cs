using System;
using System.Threading.Tasks;

namespace Red
{
    internal class HandlerWrapper
    {
        internal HandlerWrapper(string path, string method, Func<Request, Response, Task<Response.Type>>[] handlers)
        {
            Path = path;
            Method = method;
            _handlers = handlers;
        }
        internal readonly string Path;
        internal readonly string Method;
        private readonly Func<Request, Response, Task<Response.Type>>[] _handlers;
        
        internal async Task<Response.Type> Invoke(Request req, Response res)
        {
            var status = Response.Type.Continue;
            foreach (var handler in _handlers)
            {
                status = await handler(req, res);
                if (status != Response.Type.Continue) break;
            }

            return status;
        }

    }
}