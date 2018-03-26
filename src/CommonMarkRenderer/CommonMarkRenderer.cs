using System;
using System.Net;
using System.Threading.Tasks;

namespace Red.CommonMarkRenderer
{
    /// <summary>
    ///     CommonMark Renderer using CommonMark.NET converter
    /// </summary>
    public static class CommonMarkRenderer
    {
        /// <summary>
        ///     Renders a CommonMark file and sends it
        /// </summary>
        /// <param name="instance">The instance of the response</param>
        /// <param name="filePath">The path of the CommonMark file to be rendered</param>
        /// <param name="status">The status code for the response</param>
        public static Task RenderFile(this Response instance, string filePath, HttpStatusCode status = HttpStatusCode.OK)
        {
            instance.UnderlyingResponse.StatusCode = (int)status;
            instance.UnderlyingResponse.ContentType = "text/html";
            using (var reader = new System.IO.StreamReader(filePath))
            using (var writer = new System.IO.StreamWriter(instance.UnderlyingResponse.Body))
            {
                CommonMark.CommonMarkConverter.Convert(reader, writer);
            }
            instance.Closed = true;
            return Task.CompletedTask;
        }
        
        /// <summary>
        ///     Renders a string containing CommonMark text to HTML and sends it
        /// </summary>
        /// <param name="instance">The instance of the response</param>
        /// <param name="commonMarkText">The CommonMark text to be converted to HTML and sent</param>
        /// <param name="fileName">The filename to specify in response header. Optional</param>
        /// <param name="status">The status code for the response</param>
        /// <returns></returns>
        public static async Task RenderString(this Response instance, string commonMarkText, string fileName = null, HttpStatusCode status = HttpStatusCode.OK)
        {
            instance.UnderlyingResponse.StatusCode = (int)status;
            instance.UnderlyingResponse.ContentType = "text/html";
            var html = CommonMark.CommonMarkConverter.Convert(commonMarkText);
            await instance.SendString(html, "text/html", fileName, status);
        }

    }
}
