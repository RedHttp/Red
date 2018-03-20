using System;
using System.Threading.Tasks;

namespace Red
{
    class HandlerWrapper
    {
        public HandlerWrapper(string path, HttpMethodEnum method, Func<Request, Response, Task> handler)
        {
            Path = path;
            Method = method;
            Handler = handler;
        }
        public readonly string Path;
        public readonly HttpMethodEnum Method;
        public readonly Func<Request, Response, Task> Handler;
    }
}