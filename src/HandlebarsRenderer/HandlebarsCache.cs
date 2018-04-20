using System;
using System.Collections.Concurrent;
using System.IO;
using HandlebarsDotNet;

namespace Red.HandlebarsRenderer
{
    internal class HandlebarsCache
    {
        class HandlebarsCacheFile
        {
            public HandlebarsCacheFile(string filePath)
            {
                _filePath = filePath;
                Update();
            }

            private void Update()
            {
                using (var reader = File.OpenText(_filePath))
                {
                    _renderer = Handlebars.Compile(reader);
                    _updated = DateTime.UtcNow;
                }
            }

            public Action<TextWriter, object> Renderer
            {
                get
                {
                    if (File.GetLastWriteTimeUtc(_filePath) > _updated)
                        Update();
                    return _renderer;
                }
            }

            private DateTime _updated;
            private readonly string _filePath;
            private Action<TextWriter, object> _renderer;
        }

        public static HandlebarsCache Instance => _instance ?? (_instance = new HandlebarsCache());
        private static HandlebarsCache _instance;

        public Action<TextWriter, object> GetRenderer(string filePath)
        {
            if (_cachedFiles.TryGetValue(filePath, out var renderer)) 
                return renderer.Renderer;
            renderer = new HandlebarsCacheFile(filePath);
            _cachedFiles.TryAdd(filePath, renderer);
            return renderer.Renderer;
        }
        
        private readonly ConcurrentDictionary<string, HandlebarsCacheFile> _cachedFiles =
            new ConcurrentDictionary<string, HandlebarsCacheFile>();
    }
}