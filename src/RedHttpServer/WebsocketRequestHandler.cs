using System;
using System.Threading.Tasks;

namespace Red
{
    internal sealed class WebsocketRequestHandler
    {
        internal WebsocketRequestHandler(string path, Func<Request, Response, WebSocketDialog, Task<HandlerType>>[] handlers)
        {
            Path = path;
            _handlers = handlers;
        }

        internal async Task<HandlerType> Invoke(Request req, WebSocketDialog wsd, Response res)
        {
            var status = HandlerType.Continue;
            foreach (var handler in _handlers)
            {
                status = await handler(req, res, wsd);
                if (status != HandlerType.Continue) break;
            }
            return status;
        }
        
        internal readonly string Path;
        private readonly Func<Request, Response, WebSocketDialog, Task<HandlerType>>[] _handlers;
    }
}