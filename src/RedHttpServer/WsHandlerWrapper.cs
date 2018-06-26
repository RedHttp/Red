using System;

namespace Red
{
    internal class WsHandlerWrapper
    {
        public WsHandlerWrapper(string path, Action<Request, WebSocketDialog, Response>[] handlers)
        {
            Path = path;
            _handlers = handlers;
        }
        public WsHandlerWrapper(string path, Action<Request, WebSocketDialog>[] handlers)
        {
            Path = path;
            _simple = true;
            
            _simpleHandlers = handlers;
        }


        public void Process(Request req, WebSocketDialog wsd, Response res)
        {
            if (_simple)
            {
                foreach (var handler in _simpleHandlers)
                {
                    if (res.Closed) break;
                    handler(req, wsd);
                }
            }
            else
            {
                foreach (var handler in _handlers)
                {
                    if (res.Closed) break;
                    handler(req, wsd, res);
                }
            }
        }

        private readonly bool _simple;
        
        public readonly string Path;
        private readonly Action<Request, WebSocketDialog, Response>[] _handlers;
        private readonly Action<Request, WebSocketDialog>[] _simpleHandlers;
    }
}