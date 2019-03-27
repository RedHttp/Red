using System;
using System.Threading.Tasks;

namespace Red
{
    internal sealed class WsHandlerWrapper
    {
        internal WsHandlerWrapper(string path, Func<Request, Response, WebSocketDialog, Task<Response.Type>>[] handlers)
        {
            Path = path;
            _handlers = handlers;
        }

        internal WsHandlerWrapper(string path, Func<Request, WebSocketDialog, Task<Response.Type>>[] handlers)
        {
            Path = path;
            _simple = true;
            _simpleHandlers = handlers;
        }


        internal async Task<Response.Type> Invoke(Request req, WebSocketDialog wsd, Response res)
        {
            var status = Response.Type.Continue;
            if (_simple)
            {
                foreach (var handler in _simpleHandlers)
                {
                    status = await handler(req, wsd);
                    if (status != Response.Type.Continue) break;
                }
            }
            else
            {
                foreach (var handler in _handlers)
                {
                    status = await handler(req, res, wsd);
                    if (status != Response.Type.Continue) break;
                }
            }

            return status;
        }

        private readonly bool _simple;
        
        internal readonly string Path;
        private readonly Func<Request, Response, WebSocketDialog, Task<Response.Type>>[] _handlers;
        private readonly Func<Request, WebSocketDialog, Task<Response.Type>>[] _simpleHandlers;
    }
}