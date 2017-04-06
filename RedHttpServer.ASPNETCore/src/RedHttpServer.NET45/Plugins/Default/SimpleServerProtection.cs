using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RHttpServer.Logging;

namespace RHttpServer.Default
{
    /// <summary>
    ///     The default security handler. Can protect against DOS attacks
    /// </summary>
    internal sealed class SimpleServerProtection : RPlugin, IHttpSecurityHandler
    {
        internal SimpleServerProtection()
        {
            _visitorMaintainerThread = new Thread(MaintainVisitorList) {Name = "VisitorMaintainer"};
            _blacklistMaintainerThread = new Thread(MaintainBlacklist) {Name = "BlacklistMaintainer"};
        }

        private readonly ConcurrentDictionary<string, byte> _blacklist = new ConcurrentDictionary<string, byte>();
        private readonly Thread _blacklistMaintainerThread;
        private readonly Thread _visitorMaintainerThread;

        private readonly ConcurrentDictionary<string, HttpRequester> _visitors =
            new ConcurrentDictionary<string, HttpRequester>();

        private volatile bool _maintainerRunning;
        private bool _started;

        private async void MaintainVisitorList()
        {
            while (_maintainerRunning)
            {
                await Task.Delay(TimeSpan.FromSeconds(Settings.SessionLengthSeconds / 2.0));
                var now = DateTime.UtcNow;
                var olds =
                    _visitors.Where(
                            t => now.Subtract(t.Value.SessionStarted).TotalSeconds > Settings.SessionLengthSeconds)
                        .Select(t => t.Key);
                foreach (var ipAddress in olds)
                {
                    HttpRequester vis = null;
                    _visitors.TryRemove(ipAddress, out vis);
                }
            }
        }

        private async void MaintainBlacklist()
        {
            while (_maintainerRunning)
            {
                await Task.Delay(TimeSpan.FromMinutes(Settings.BanTimeMinutes / 2.0));
                var now = DateTime.UtcNow;
                var olds =
                    _visitors.Where(
                            t => now.Subtract(t.Value.SessionStarted).TotalSeconds > Settings.SessionLengthSeconds)
                        .Select(t => t.Key);
                foreach (var ipAddress in olds)
                {
                    HttpRequester vis = null;
                    _visitors.TryRemove(ipAddress, out vis);
                }
            }
        }

        public bool HandleRequest(HttpListenerRequest req)
        {
            var url = req.Headers["X-Real-IP"];
            if (string.IsNullOrEmpty(url))
                url = req.RemoteEndPoint?.Address.ToString();
            if (url == null || Settings == null) return true;
            HttpRequester vis;
            if (_blacklist.ContainsKey(url)) return false;

            if (_visitors.TryGetValue(url, out vis))
            {
                if (vis.JustRequested() <= Settings.MaxRequestsPerSession) return true;
                _blacklist.TryAdd(url, 1);
                _visitors.TryRemove(url, out vis);

                Logger.Log("SEC", $"{url} has been blacklisted for {Settings.BanTimeMinutes} minutes");
                return true;
            }
            _visitors.TryAdd(url, new HttpRequester());
            return true;
        }

        public void Start()
        {
            if (_started) return;
            _maintainerRunning = true;
            _visitorMaintainerThread.Start();
            _blacklistMaintainerThread.Start();
            _started = true;
        }

        public void Stop()
        {
            if (!_started) return;
            _maintainerRunning = false;
            _visitorMaintainerThread.Join(100);
            _blacklistMaintainerThread.Join(100);
            _started = false;
        }

        public IHttpSecuritySettings Settings { get; set; }

        /// <summary>
        ///     Represents a http client in the DOS security implementation
        /// </summary>
        internal class HttpRequester
        {
            internal HttpRequester()
            {
                RequestsInSession = 1;
                SessionStarted = DateTime.UtcNow;
            }

            private readonly object _lock = new object();

            /// <summary>
            ///     When the current request session was started
            /// </summary>
            public DateTime SessionStarted { get; }

            /// <summary>
            ///     The number of requests since session start
            /// </summary>
            public int RequestsInSession { get; private set; }

            /// <summary>
            ///     When the latest request from client was received
            /// </summary>
            public DateTime LatestVisit { get; private set; }

            /// <summary>
            ///     Called everytime a client requests something
            /// </summary>
            /// <returns></returns>
            public int JustRequested()
            {
                lock (_lock)
                {
                    RequestsInSession++;
                }
                LatestVisit = DateTime.UtcNow;
                return RequestsInSession;
            }
        }
    }
}