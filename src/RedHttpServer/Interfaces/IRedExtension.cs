namespace Red.Interfaces
{
    /// <summary>
    ///     Interface for all extension modules for Red.
    /// </summary>
    public interface IRedExtension
    {
        /// <summary>
        ///     Called on all registered plugins/middleware when the server is started.
        ///     The module is handed a reference to the server, so it can do any needed registration
        /// </summary>
        /// <param name="server">A reference to the instance of the RedHttpServer</param>
        void Initialize(RedHttpServer server);
    }
}