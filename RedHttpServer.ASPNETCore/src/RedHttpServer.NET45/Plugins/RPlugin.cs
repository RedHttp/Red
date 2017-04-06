namespace RHttpServer
{
    /// <summary>
    ///     Abstract class that all plugins to be registered in RHttpServer must derive from
    /// </summary>
    public abstract class RPlugin
    {
        private RPluginCollection _pluginCollection;

        internal void SetPlugins(RPluginCollection pluginCollection)
        {
            _pluginCollection = pluginCollection;
        }

        /// <summary>
        ///     Use a plugin registered to the server
        /// </summary>
        /// <typeparam name="TPluginInterface">The type the plugin implements</typeparam>
        /// <returns>The instance of the plugin</returns>
        protected TPluginInterface UsePlugin<TPluginInterface>()
        {
            return _pluginCollection.Use<TPluginInterface>();
        }
    }
}