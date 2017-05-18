using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace RedHttpServerNet45.Request
{
    /// <summary>
    /// Key-Value collection of form-data inputs
    /// </summary>
    public class RFormCollection
    {
        internal RFormCollection()
        {
            
        }

        private readonly Dictionary<string, List<string>> _dict = new Dictionary<string, List<string>>();
        private static readonly IReadOnlyList<string> EmptyList = new List<string>();

        internal void Add(string key, string value)
        {
            List<string> l;
            if (!_dict.TryGetValue(key, out l))
                _dict.Add(key, l = new List<string>());
            l.Add(value);
        }

        /// <summary>
        /// Get input values from field with specicied key
        /// </summary>
        /// <param name="key">Key to lookup</param>
        /// <returns>List of string associated with key</returns>
        public IReadOnlyList<string> this[string key]
        {
            get
            {
                List<string> l;
                if (!_dict.TryGetValue(key, out l))
                    return EmptyList;
                return l;
            }
        }
    }

    internal static class FormDataParser
    {
        private static readonly Regex NameRegex = new Regex("name=\"(.+)\"", RegexOptions.Compiled);

        internal static async Task<RFormCollection> ParseMultipart(Stream stream, string boundary)
        {
            var fc = new RFormCollection();
            using (var reader = new StreamReader(stream))
            {
                var line = await reader.ReadLineAsync();
                var bEnd = boundary + "--";
                while (!line.Contains(bEnd) && !reader.EndOfStream)
                {
                    line = await reader.ReadLineAsync();
                    if (line.Contains("Content-Disposition: form-data"))
                    {
                        var m = NameRegex.Match(line);
                        if (m.Success)
                        {
                            var name = m.Groups[1].Value;
                            var sb = new StringBuilder();
                            line = await reader.ReadLineAsync();
                            while (!line.Contains(boundary))
                            {
                                sb.AppendLine(line);
                                line = await reader.ReadLineAsync();
                            }
                            fc.Add(name, sb.ToString().Trim(' ', '\r', '\n'));
                        }
                        else
                            while (!line.Contains(boundary))
                                line = await reader.ReadLineAsync();
                    }
                    else
                        while (!line.Contains(boundary))
                            line = await reader.ReadLineAsync();
                }
            }
            return fc;
        }

        internal static async Task<RFormCollection> ParseUrlEncoded(Stream stream)
        {
            var fc = new RFormCollection();
            var data = "";
            using (var reader = new StreamReader(stream))
                data = await reader.ReadToEndAsync();
            data = data.TrimStart('?');
            var entries = data.Split('&');
            foreach (var entry in entries)
            {
                var d = entry.Split('=');
                var key = d[0];
                var values = d[1].Split(',');
                foreach (var value in values)
                    fc.Add(key, value.Trim());
            }
            return fc;
        }
    }
}