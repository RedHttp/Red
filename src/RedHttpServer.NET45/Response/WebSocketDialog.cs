using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedHttpServerNet45.Response
{
    /// <summary>
    ///     Represents a websocket dialog between server and a client
    /// </summary>
    public class WebSocketDialog
    {
        internal WebSocketDialog(WebSocketContext wsc, RPluginCollection plugins)
        {
            UnderlyingContext = wsc;
            _ws = UnderlyingContext.WebSocket;
            ServerPlugins = plugins;
        }

        public RPluginCollection ServerPlugins { get; set; }

        private readonly WebSocket _ws;

        /// <summary>
        ///     The underlying WebSocketContext
        /// </summary>
        public WebSocketContext UnderlyingContext { get; set; }

        /// <summary>
        ///     Raised when binary websocket messages are received
        /// </summary>
        public event EventHandler<BinaryMessageEventArgs> OnBinaryReceived;

        /// <summary>
        ///     Raised when text messages are received
        /// </summary>
        public event EventHandler<TextMessageEventArgs> OnTextReceived;

        /// <summary>
        ///     Raised when socket is closed
        /// </summary>
        public event EventHandler OnClosed;

        /// <summary>
        ///     Send text message using websocket
        /// </summary>
        /// <param name="text"></param>
        /// <param name="endOfMessage"></param>
        public async Task SendText(string text, bool endOfMessage = true)
        {
            await _ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(text)), WebSocketMessageType.Text,
                endOfMessage,
                CancellationToken.None);
        }

        /// <summary>
        ///     Send binary data using the websocket
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endOfMessage"></param>
        public async Task SendBinary(ArraySegment<byte> data, bool endOfMessage = true)
        {
            await _ws.SendAsync(data, WebSocketMessageType.Binary, endOfMessage,
                CancellationToken.None);
        }

        /// <summary>
        ///     Closes the WebSocket connection
        /// </summary>
        /// <param name="status"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public async Task Close(WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
            string description = "")
        {
            await _ws.CloseAsync(status, description, CancellationToken.None);
        }

        internal async Task ReadFromWebSocket()
        {
            var buffer = new byte[0x2000];
            try
            {
                while (_ws.State == WebSocketState.Open)
                {
                    var received = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    switch (received.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            OnTextReceived?.Invoke(this,
                                new TextMessageEventArgs(Encoding.UTF8.GetString(buffer, 0, received.Count),
                                    received.EndOfMessage));
                            break;
                        case WebSocketMessageType.Binary:
                            OnBinaryReceived?.Invoke(this,
                                new BinaryMessageEventArgs(new ArraySegment<byte>(buffer, 0, received.Count),
                                    received.EndOfMessage));
                            break;
                        case WebSocketMessageType.Close:
                            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                            break;
                    }
                }
            }
            catch (WebSocketException)
            {
            }
            finally
            {
                OnClosed?.Invoke(this, EventArgs.Empty);
                _ws.Dispose();
            }
        }

        /// <summary>
        ///     Represents a binary message received from websocket
        /// </summary>
        public class BinaryMessageEventArgs : EventArgs
        {
            internal BinaryMessageEventArgs(ArraySegment<byte> data, bool endOfMessage)
            {
                _as = data;
                EndOfMessage = endOfMessage;
            }

            private ArraySegment<byte> _as;

            /// <summary>
            ///     The binary content of the message
            /// </summary>
            public byte[] Data => _as.Array;

            /// <summary>
            ///     Whether this is a complete message or the end of one, or there is more to come.
            /// </summary>
            public bool EndOfMessage { get; }
        }

        /// <summary>
        ///     Represents a UTF-8 encoded text message received from websocket
        /// </summary>
        public class TextMessageEventArgs : EventArgs
        {
            internal TextMessageEventArgs(string text, bool endOfMessage)
            {
                Text = text;
                EndOfMessage = endOfMessage;
            }

            /// <summary>
            ///     The text content of the message
            /// </summary>
            public string Text { get; }

            /// <summary>
            ///     Whether this is a complete message or the end of one, or there is more to come.
            /// </summary>
            public bool EndOfMessage { get; }
        }
    }
}