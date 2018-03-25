using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Red.Interfaces;

namespace EcsRendererPlugin
{

    /// <summary>
    ///     Renderer for pages using ecs tags ("ecs files")
    /// </summary>
    internal sealed class EcsPageRenderer
    {
        class CachedFile
        {
            private readonly string _filePath;
            private DateTime _loaded = DateTime.MinValue;
            private string _content = "";

            public CachedFile(string filePath)
            {
                _filePath = filePath;
            }

            public string Get()
            {
                if (_loaded < File.GetLastWriteTimeUtc(_filePath))
                {
                    if (Path.GetExtension(_filePath).ToLowerInvariant() == ".ecs")
                    {
                        var sb = new StringBuilder(File.ReadAllText(_filePath));
                        ReplaceFileTags(sb);
                        _content = sb.ToString();
                    }
                    else
                        _content = File.ReadAllText(_filePath);
                    _loaded = DateTime.UtcNow;
                }

                return _content;
            }
        }
        private static readonly Regex NormalTagRegex =
            new Regex(@"<% *([\w-]+) *%>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex HtmlTagRegex =
            new Regex(@"<%= *([\w-]+) *=%>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex FileTagRegex =
            new Regex(@"<%- *((?:[\w]\:|\\)(\\[a-z_\-\s0-9\.]+)+\.(html|ecs|js|css|txt)) *-%>",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly ConcurrentDictionary<string, CachedFile> _renderCache = new ConcurrentDictionary<string, CachedFile>();
        private readonly bool _cachePages;

        public EcsPageRenderer(bool renderCaching)
        {
            _cachePages = renderCaching;
        }

        private static string InternalRenderNoCache(StringBuilder pageContent, RenderParams parameters)
        {
            ReplaceFileTags(pageContent);
            if (parameters != null)
                ReplaceTags(pageContent, parameters);
            return pageContent.ToString();
        }

        private static void InternalRenderFileOnly(StringBuilder sb)
        {
            var doneHashset = new HashSet<string>();
            var matches = FileTagRegex.Matches(sb.ToString());
            foreach (Match match in matches)
            {
                if (doneHashset.Contains(match.Value)) continue;
                var m = match.Groups[1].Value.Trim('<', '>', '�', ' ');
                if (!File.Exists(m))
                    continue;
                var rfile = new StringBuilder(File.ReadAllText(m));
                if (Path.GetExtension(m) == ".ecs")
                    InternalRenderFileOnly(rfile);

                sb.Replace(match.Value, rfile.ToString());
                doneHashset.Add(match.Value);
            }
        }

        private static void ReplaceTags(StringBuilder sb, RenderParams parameters)
        {
            var doneHashset = new HashSet<string>();
            var pmatches = NormalTagRegex.Matches(sb.ToString());
            foreach (Match pmatch in pmatches)
            {
                if (doneHashset.Contains(pmatch.Value)) continue;
                var tag = pmatch.Groups[1].Value;
                var data = parameters[tag];
                if (data == null) continue;
                sb.Replace(pmatch.Value, data);
                doneHashset.Add(pmatch.Value);
            }

            pmatches = HtmlTagRegex.Matches(sb.ToString());
            foreach (Match pmatch in pmatches)
            {
                if (doneHashset.Contains(pmatch.Value)) continue;
                var m = pmatch.Groups[1].Value;
                var t = parameters[m];
                if (string.IsNullOrEmpty(t)) continue;
                sb.Replace(pmatch.Value, HtmlEncoder.Default.Encode(t));
                doneHashset.Add(pmatch.Value);
            }
        }

        private static void ReplaceFileTags(StringBuilder sb)
        {
            var doneHashset = new HashSet<string>();
            var matches = FileTagRegex.Matches(sb.ToString());
            foreach (Match match in matches)
            {
                if (doneHashset.Contains(match.Value)) continue;
                var file = match.Groups[1].Value;
                if (!File.Exists(file)) continue;
                if (Path.GetExtension(file).ToLowerInvariant() == ".ecs")
                {
                    var rfile = new StringBuilder(File.ReadAllText(file));
                    ReplaceFileTags(rfile);
                    sb.Replace(match.Value, rfile.ToString());
                }
                else
                    sb.Replace(match.Value, File.ReadAllText(file));

                doneHashset.Add(match.Value);
            }
        }

        private static string InternalRenderLite(StringBuilder sb, RenderParams parameters)
        {
            if (parameters == null) return sb.ToString();
            ReplaceTags(sb, parameters);
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
            if (_cachePages)
            {
                if (!_renderCache.TryGetValue(filepath, out var cached))
                {
                    cached = new CachedFile(filepath);
                    _renderCache.TryAdd(filepath, cached);
                }
                var sb = new StringBuilder(cached.Get());
                return InternalRenderLite(sb, parameters);
            }
            return InternalRenderNoCache(new StringBuilder(File.ReadAllText(filepath)), parameters);
        }
    }
}