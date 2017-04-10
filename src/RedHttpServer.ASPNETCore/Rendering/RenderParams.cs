using System.Collections;
using System.Collections.Generic;
using RedHttpServer.Plugins;
using RedHttpServer.Plugins.Interfaces;
using ServiceStack.Text;

namespace RedHttpServer.Rendering
{    
    /// <summary>
    ///     Parameters used when rendering a page
    /// </summary>
    public sealed class RenderParams : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly IDictionary<string, string> _dict = new Dictionary<string, string>();

        /// <summary>
        ///     Attempts to retrieve the replacement data associated with the tag
        ///     Returns empty string if tag not found in parameters
        /// </summary>
        /// <param name="parName">The name of the tag to find replacement data for</param>
        /// <returns>Replacement data for the given tag</returns>
        public string this[string parName]
        {
            get
            {
                var res = "";
                _dict.TryGetValue(parName, out res);
                return res;
            }
        }

        internal static IJsonConverter Converter { get; set; }

        /// <summary>
        ///     Adds tag and replacement-data pair to parameters
        /// </summary>
        /// <param name="parTag">The tag id</param>
        /// <param name="parData">The replacement-data for the tag</param>
        public void Add(string parTag, string parData)
        {
            _dict.Add(new KeyValuePair<string, string>(parTag, parData));
        }

        /// <summary>
        ///     Adds tag and json-serialized replacement-data pair to parameters
        /// </summary>
        /// <param name="parTag">The tag id</param>
        /// <param name="parData">The replacement-data object for the tag</param>
        public void Add(string parTag, object parData)
        {
            _dict.Add(new KeyValuePair<string, string>(parTag, Converter.Serialize(parData)));
        }

        /// <summary>
        ///     Returns the enumeration of tag and replacement-data pairs
        /// </summary>
        /// <returns>Pairs of tag and replacement-data</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}