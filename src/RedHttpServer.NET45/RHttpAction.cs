using System;
using System.Collections.Generic;
using RedHttpServer.Request;
using RedHttpServer.Response;

namespace RedHttpServer
{
    internal sealed class RHttpAction
    {
        internal RHttpAction(string route, Action<RRequest, RResponse> action)
        {
            RouteTree = route.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            Action = action;
            for (var i = 0; i < RouteTree.Length; i++)
            {
                var s = RouteTree[i];
                if (!s.StartsWith(":")) continue;
                Params.Add(new KeyValuePair<int, string>(i, s.TrimStart(':')));
                RouteTree[i] = "^";
            }
        }

        internal RHttpAction(string route, Action<RRequest, WebSocketDialog> action, string protocol)
        {
            RouteTree = route.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            WSAction = action;
            WSProtocol = protocol;
            for (var i = 0; i < RouteTree.Length; i++)
            {
                var s = RouteTree[i];
                if (!s.StartsWith(":")) continue;
                Params.Add(new KeyValuePair<int, string>(i, s.TrimStart(':')));
                RouteTree[i] = "^";
            }
        }

        internal readonly string[] RouteTree;
        internal List<KeyValuePair<int, string>> Params { get; } = new List<KeyValuePair<int, string>>();
        internal Action<RRequest, RResponse> Action { get; }
        internal Action<RRequest, WebSocketDialog> WSAction { get; }
        internal string WSProtocol { get; }
    }
}