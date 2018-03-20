using Red;
using Red.Plugins.Interfaces;

namespace EcsRendererPlugin
{
    public class EcsRenderer : IRedExtension
    {
        private readonly bool _renderCaching;

        public EcsRenderer(bool renderCaching = false)
        {
            _renderCaching = renderCaching;
        }
        
        public void Initialize(RedHttpServer server)
        {
            var renderer = new EcsPageRenderer(_renderCaching, server.Plugins.Get<IJsonConverter>());
            server.Plugins.Register(renderer);
        }
    }
}