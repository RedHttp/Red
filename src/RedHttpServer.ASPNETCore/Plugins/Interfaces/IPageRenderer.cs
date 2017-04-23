using RedHttpServerCore.Response;

namespace RedHttpServerCore.Plugins.Interfaces
{
    /// <summary>
    ///     The interface used for page renderer in RHttpServer
    /// </summary>
    public interface IPageRenderer
    {
        /// <summary>
        ///     Bool specifying whether the page renderer should cache pages
        /// </summary>
        bool CachePages { get; set; }

        /// <summary>
        ///     Renders the page found at path using the parameters passed.
        /// </summary>
        /// <param name="filepath">The path of the page file</param>
        /// <param name="parameters">The parameters used for rendering the page</param>
        /// <returns>Returns the content of the rendered page, ready to be sent as response</returns>
        string Render(string filepath, RenderParams parameters);
    }
}