using System;
using System.Collections.Concurrent;

namespace RHttpServer
{
    /// <summary>
    ///     A collection of RPlugins
    /// </summary>
    public sealed class RPluginCollection
    {
        internal RPluginCollection()
        {
        }

        private readonly ConcurrentDictionary<Type, object> _plugins = new ConcurrentDictionary<Type, object>();

        internal void Add(Type pluginInterface, object plugin)
        {
            if (!_plugins.TryAdd(pluginInterface, plugin))
                throw new RHttpServerException("You can only register one plugin to a plugin interface");
        }

        /// <summary>
        ///     Check whether a plugin is registered to the given type-key
        /// </summary>
        /// <typeparam name="TPluginInterface">The type-key to look-up</typeparam>
        /// <returns>Whether the any plugin is registered to TPluginInterface</returns>
        public bool IsRegistered<TPluginInterface>() => _plugins.ContainsKey(typeof(TPluginInterface));

        /// <summary>
        ///     Returns the instance of the registered plugin
        /// </summary>
        /// <typeparam name="TPluginInterface">The type-key to look-up</typeparam>
        /// <exception cref="RHttpServerException">Throws exception when trying to use a plugin that is not registered</exception>
        public TPluginInterface Use<TPluginInterface>()
        {
            object obj;
            if (_plugins.TryGetValue(typeof(TPluginInterface), out obj)) return (TPluginInterface) obj;
            throw new RHttpServerException(
                $"You must have registered a plugin that implements '{typeof(TPluginInterface).Name}'");
        }
    }
}