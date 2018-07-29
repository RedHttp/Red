using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Red
{
    /// <summary>
    ///     Represents a websocket dialog between the server and a client.
    ///     The WebSocketDialog will only start reading from the websocket after the handlers have been processed.
    /// </summary>
    public sealed class WebSocketDialog
    {
        internal WebSocketDialog(HttpContext ctx, WebSocket underlyingWebSocket, PluginCollection plugins)
        {
            UnderlyingRequest = ctx.Request;
            UnderlyingWebSocket = underlyingWebSocket;
            ServerPlugins = plugins;
        }

        /// <summary>
        ///     The underlying WebSocket
        /// </summary>
        public readonly WebSocket UnderlyingWebSocket;

        /// <summary>
        ///     The available plugins registered to the server
        /// </summary>
        public readonly PluginCollection ServerPlugins;

        /// <summary>
        ///     The underlying HttpRequest
        /// </summary>
        public readonly HttpRequest UnderlyingRequest;

        /// <summary>
        ///     Raised when binary WebSocket messages are received
        /// </summary>
        public event EventHandler<BinaryMessageEventArgs> OnBinaryReceived;

        /// <summary>
        ///     Raised when text WebSocket messages are received
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
            await UnderlyingWebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(text)),
                WebSocketMessageType.Text, endOfMessage, CancellationToken.None);
        }

        /// <summary>
        ///     Send binary message using websocket
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endOfMessage"></param>
        public async Task SendBytes(ArraySegment<byte> data, bool endOfMessage = true)
        {
            await UnderlyingWebSocket.SendAsync(data, WebSocketMessageType.Binary, endOfMessage,
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
            await UnderlyingWebSocket.CloseAsync(status, description, CancellationToken.None);
        }

        internal async Task ReadFromWebSocket()
        {
            var buffer = new byte[0x1000];
            try
            {
                var received =
                    await UnderlyingWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                while (!received.CloseStatus.HasValue)
                {
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
                            await UnderlyingWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "",
                                CancellationToken.None);
                            break;
                    }

                    received = await UnderlyingWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer),
                        CancellationToken.None);
                }
            }
            catch (WebSocketException)
            {
            }
            finally
            {
                OnClosed?.Invoke(this, EventArgs.Empty);
                await UnderlyingWebSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "",
                    CancellationToken.None);
                UnderlyingWebSocket.Dispose();
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