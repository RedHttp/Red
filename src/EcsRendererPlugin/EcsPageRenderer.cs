using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Red.Interfaces;

namespace EcsRendererPlugin
{
    /// <summary>
    ///     Renderer for pages using ecs tags ("ecs files")
    /// </summary>
    public sealed class EcsPageRenderer
    {
        private static readonly Regex NormalTagRegex =
            new Regex(@"(?i)<% ?[a-z_][a-z_0-9]* ?%>", RegexOptions.Compiled);

        private static readonly Regex HtmlTagRegex =
            new Regex(@"(?i)<%= ?[a-z_][a-z_0-9]* ?=%>", RegexOptions.Compiled);

        private static readonly Regex FileTagRegex =
            new Regex(@"(?i)<� ?([a-z]:|.)?[.\\\/\w]+.(html|ecs|js|css|txt) ?�>", RegexOptions.Compiled);

        private readonly ConcurrentDictionary<string, string> _renderCache = new ConcurrentDictionary<string, string>();
        private readonly bool _cachePages;

        public EcsPageRenderer(bool renderCaching)
        {
            _cachePages = renderCaching;
        }

        private static string InternalRenderNoCacheRec(StringBuilder pageContent, RenderParams parameters)
        {
            var doneHashset = new HashSet<string>();
            if (parameters != null)
            {
                var pmatches = NormalTagRegex.Matches(pageContent.ToString());
                foreach (Match pmatch in pmatches)
                {
                    if (doneHashset.Contains(pmatch.Value)) continue;
                    var m = pmatch.Value.Trim('<', '>', '%', ' ');
                    var t = parameters[m];
                    if (t == null) continue;
                    pageContent.Replace(pmatch.Value, t);
                    doneHashset.Add(pmatch.Value);
                }
                pmatches = HtmlTagRegex.Matches(pageContent.ToString());
                foreach (Match pmatch in pmatches)
                {
                    if (doneHashset.Contains(pmatch.Value)) continue;
                    var m = pmatch.Value.Trim('<', '>', '%', '=', ' ');
                    var t = parameters[m];
                    if (string.IsNullOrEmpty(t)) continue;
                    pageContent.Replace(pmatch.Value, System.Net.WebUtility.HtmlEncode(t));
                    doneHashset.Add(pmatch.Value);
                }
            }


            var matches = FileTagRegex.Matches(pageContent.ToString());
            foreach (Match match in matches)
            {
                if (doneHashset.Contains(match.Value)) continue;
                var m = match.Value.Trim('<', '>', '�', ' ');
                StringBuilder rfile;
                if (File.Exists(m))
                    rfile = new StringBuilder(File.ReadAllText(m));
                else
                    continue;

                pageContent.Replace(match.Value,
                    Path.GetExtension(m) == ".ecs"
                        ? InternalRenderNoCacheRec(rfile, parameters)
                        : rfile.ToString());
                doneHashset.Add(match.Value);
            }
            return pageContent.ToString();
        }

        private static void InternalRenderFileOnly(StringBuilder sb)
        {
            var doneHashset = new HashSet<string>();
            var matches = FileTagRegex.Matches(sb.ToString());
            foreach (Match match in matches)
            {
                if (doneHashset.Contains(match.Value)) continue;
                var m = match.Value.Trim('<', '>', '�', ' ');
                if (!File.Exists(m))
                    continue;
                var rfile = new StringBuilder(File.ReadAllText(m));
                if (Path.GetExtension(m) == ".ecs")
                    InternalRenderFileOnly(rfile);

                sb.Replace(match.Value, rfile.ToString());
                doneHashset.Add(match.Value);
            }
        }

        private static string InternalRenderLite(StringBuilder sb, RenderParams parameters)
        {
            var doneHashset = new HashSet<string>();
            if (parameters == null) return sb.ToString();
            var pmatches = NormalTagRegex.Matches(sb.ToString());
            foreach (Match pmatch in pmatches)
            {
                if (doneHashset.Contains(pmatch.Value)) continue;
                var m = pmatch.Value.Trim('<', '>', '%', ' ');
                var t = parameters[m];
                if (t == null) continue;
                sb.Replace(pmatch.Value, t);
                doneHashset.Add(pmatch.Value);
            }
            pmatches = HtmlTagRegex.Matches(sb.ToString());
            foreach (Match pmatch in pmatches)
            {
                if (doneHashset.Contains(pmatch.Value)) continue;
                var m = pmatch.Value.Trim('<', '>', '%', '=', ' ');
                var t = parameters[m];
                if (string.IsNullOrEmpty(t)) continue;
                sb.Replace(pmatch.Value, System.Net.WebUtility.HtmlEncode(t));
                doneHashset.Add(pmatch.Value);
            }
            return sb.ToString();
        }

        /// <summary>
        ///     Renders the ecs file at the given path
        /// </summary>
        /// <param name="filepath">ecs file path</param>
        /// <param name="parameters">Rendering parameter</param>
        /// <returns></returns>
        public string Render(string filepath, RenderParams parameters)
        {
            if (Path.GetExtension(filepath) != ".ecs")
                throw new ArgumentException("Please use .ecs files for rendering");

            if (_cachePages && _renderCache.TryGetValue(filepath, out var cached))
                return InternalRenderLite(new StringBuilder(cached), parameters);
            var sb = new StringBuilder(File.ReadAllText(filepath));
            if (!_cachePages)
                return InternalRenderNoCacheRec(sb, parameters);
            InternalRenderFileOnly(sb);
            _renderCache.TryAdd(filepath, sb.ToString());
            return InternalRenderLite(sb, parameters);
        }
    }
}