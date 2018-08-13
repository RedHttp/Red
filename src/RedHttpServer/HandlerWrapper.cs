using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Red
{
    internal class HandlerWrapper
    {
        public HandlerWrapper(string path, string method, Func<Request, Response, Task>[] handlers)
        {
            Path = path;
            Method = method;
            _handlers = handlers;
        }
        public readonly string Path;
        public readonly string Method;
        private readonly Func<Request, Response, Task>[] _handlers;
        
        public async Task Invoke(Request req, Response res)
        {
            foreach (var handler in _handlers)
            {
                if (res.Closed) break;
                await handler(req, res);
            }
        }

    }
}