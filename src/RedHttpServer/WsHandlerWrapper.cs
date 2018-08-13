using System;
using System.Threading.Tasks;

namespace Red
{
    internal class WsHandlerWrapper
    {
        public WsHandlerWrapper(string path, Func<Request, Response, WebSocketDialog, Task>[] handlers)
        {
            Path = path;
            _handlers = handlers;
        }

        public WsHandlerWrapper(string path, Func<Request, WebSocketDialog, Task>[] handlers)
        {
            Path = path;
            _simple = true;
            _simpleHandlers = handlers;
        }


        public async Task Invoke(Request req, WebSocketDialog wsd, Response res)
        {
            if (_simple)
            {
                foreach (var handler in _simpleHandlers)
                {
                    if (res.Closed) break;
                    await handler(req, wsd);
                }
            }
            else
            {
                foreach (var handler in _handlers)
                {
                    if (res.Closed) break;
                    await handler(req, res, wsd);
                }
            }
        }

        private readonly bool _simple;
        
        public readonly string Path;
        private readonly Func<Request, Response, WebSocketDialog, Task>[] _handlers;
        private readonly Func<Request, WebSocketDialog, Task>[] _simpleHandlers;
    }
}