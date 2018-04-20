using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Red.HandlebarsRenderer
{
    /// <summary>
    ///     Handlebars renderer extension using Handlebars.Net
    /// </summary>
    public static class HandlebarsRenderer
    {
        /// <summary>
        ///     Renders a Handlebars template file and sends it
        /// </summary>
        /// <param name="instance">The instance of the response</param>
        /// <param name="filePath">The path of the Handlebars template file to be rendered</param>
        /// <param name="renderParams">The render parameter object which will be passed to the template renderer</param>
        /// <param name="status">The status code for the response</param>
        public static Task RenderTemplate(this Response instance, string filePath, object renderParams,
            HttpStatusCode status = HttpStatusCode.OK)
        {
            instance.UnderlyingResponse.StatusCode = (int) status;
            instance.UnderlyingResponse.ContentType = "text/html";
            var renderer = HandlebarsCache.Instance.GetRenderer(filePath);
            using (var writer = new StreamWriter(instance.UnderlyingResponse.Body))
            {
                renderer(writer, renderParams);
            }

            instance.Closed = true;
            return Task.CompletedTask;
        }
    }
}