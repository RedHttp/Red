using System.Net;
using System.Threading.Tasks;

namespace RHttpServer
{
    /// <summary>
    ///     Http server where request are handled as tasks
    ///     <para />
    ///     Good for processing requests of varying size
    /// </summary>
    public sealed class TaskBasedHttpServer : BaseHttpServer
    {
        /// <summary>
        /// </summary>
        /// <param name="port">The port to listen on</param>
        /// <param name="path">Path to use as public dir. Set to null or empty string if none wanted</param>
        /// <param name="throwExceptions">Whether exceptions should be suppressed and logged, or thrown</param>
        public TaskBasedHttpServer(int port, string path = "", bool throwExceptions = false)
            : base(port, path, throwExceptions)
        {
        }

        protected override void ProcessContext(HttpListenerContext context)
        {
            Task.Run(() =>
            {
                if (!SecurityOn || SecMan.HandleRequest(context.Request))
                    Process(context);
            });
        }
    }
}