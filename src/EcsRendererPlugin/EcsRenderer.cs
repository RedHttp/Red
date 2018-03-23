using Red;
using Red.Interfaces;

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
            RenderParams.Converter = server.Plugins.Get<IJsonConverter>();
            var renderer = new EcsPageRenderer(_renderCaching);
            server.Plugins.Register(renderer);
        }
    }
}