using System;
using System.Collections.Generic;

namespace RedHttpServer
{
    internal sealed class RouteTreeManager
    {
        private readonly Dictionary<HttpMethod, RouteTree> _trees = new Dictionary<HttpMethod, RouteTree>();

        internal void AddRoute(RHttpAction action, HttpMethod method)
        {
            RouteTree rt;
            if (!_trees.TryGetValue(method, out rt))
            {
                rt = new RouteTree("", null);
                _trees.Add(method, rt);
            }
            AddToTree(rt, action);
        }

        private static void AddToTree(RouteTree tree, RHttpAction action)
        {
            var rTree = action.RouteTree;
            var len = rTree.Length;
            for (var i = 0; i < len; i++)
            {
                var ntree = tree.
                    AddBranch(rTree[i]);
                tree = ntree;
            }
            if (tree.Action != null) throw new RedHttpServerException("Cannot add two actions to the same route");
            tree.Action = action;
        }

        internal RHttpAction SearchInTree(string route, HttpMethod meth, out bool atmostGeneral)
        {
            RouteTree tree;
            if (!_trees.TryGetValue(meth, out tree))
            {
                atmostGeneral = true;
                return null;
            }

            var split = route.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in split)
            {
                var branch = tree.GetBranch(s);
                if (branch != null)
                {
                    tree = branch;
                    continue;
                }
                branch = tree;
                while (branch.Route != "*")
                {
                    if (branch.Stem == null) break;
                    branch = branch.Stem;
                }
                atmostGeneral = true;
                return branch.Route == "*" ? branch.Action : null;
            }
            atmostGeneral = tree.Route == "*";
            return tree.Action;
        }
    }
}