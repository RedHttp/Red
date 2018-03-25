using System.Net;
using System.Threading.Tasks;
using Red;

namespace EcsRendererPlugin
{
    public static class EcsRendererExtension
    {
        /// <summary>
        ///     Renders an ecs page file to HTML and sends it
        /// </summary>
        /// <param name="instance">The instance of the response</param>
        /// <param name="pageFilePath">The path of the ecs file to be rendered</param>
        /// <param name="parameters">The parameter collection used when rendering the template</param>
        /// <param name="status">The status code for the response</param>
        public static async Task RenderPage(this Response instance, string pageFilePath, RenderParams parameters, HttpStatusCode status = HttpStatusCode.OK)
        {
            var page = instance.ServerPlugins.Get<EcsPageRenderer>().Render(pageFilePath, parameters);
            await instance.SendString(page, "text/html", status: status);
        }

    }
}
