using System.Net;
using System.Runtime.CompilerServices;

namespace RedHttpServer.Handling
{
    internal sealed class ActionOnlyResponseHandler : ResponseHandler
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Handle(string route, HttpListenerContext context)
        {
            return false;
        }
    }
}