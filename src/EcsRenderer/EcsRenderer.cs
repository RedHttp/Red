using Red;
using Red.Interfaces;

namespace EcsRendererPlugin
{
    /// <summary>
    ///     RedExtension for EcsRenderer
    /// </summary>
    public class EcsRenderer : IRedExtension
    {
        private readonly bool _renderCaching;

        /// <summary>
        ///     Contructor for EcsRenderer extension 
        /// </summary>
        /// <param name="renderCaching">Whether to cache files</param>
        public EcsRenderer(bool renderCaching = false)
        {
            _renderCaching = renderCaching;
        }
        
        /// <summary>
        ///     Do not invoke. Is invoked by the server when it starts. 
        /// </summary>
        /// <param name="server"></param>
        public void Initialize(RedHttpServer server)
        {
            RenderParams.Converter = server.Plugins.Get<IJsonConverter>();
            var renderer = new EcsPageRenderer(_renderCaching);
            server.Plugins.Register(renderer);
        }
    }
}