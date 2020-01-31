using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Red
{
    /// <summary>
    ///     Represents a websocket dialog between the server and a client.
    ///     The WebSocketDialog will only start reading from the websocket after the handlers have been processed.
    /// </summary>
    public sealed class WebSocketDialog
    {
        /// <summary>
        ///     The underlying WebSocket
        /// </summary>
        public readonly WebSocket WebSocket;

        internal WebSocketDialog(WebSocket webSocket)
        {
            WebSocket = webSocket;
        }


        /// <summary>
        ///     Raised when binary WebSocket messages are received
        /// </summary>
        public event EventHandler<BinaryMessageEventArgs>? OnBinaryReceived;

        /// <summary>
        ///     Raised when text WebSocket messages are received
        /// </summary>
        public event EventHandler<TextMessageEventArgs>? OnTextReceived;

        /// <summary>
        ///     Raised when socket is closed
        /// </summary>
        public event EventHandler? OnClosed;

        /// <summary>
        ///     Send text message using websocket
        /// </summary>
        /// <param name="text"></param>
        /// <param name="endOfMessage"></param>
        public Task SendText(string text, bool endOfMessage = true)
        {
            return WebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(text)),
                WebSocketMessageType.Text, endOfMessage, CancellationToken.None);
        }

        /// <summary>
        ///     Send binary message using websocket
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endOfMessage"></param>
        public Task SendBytes(ArraySegment<byte> data, bool endOfMessage = true)
        {
            return WebSocket.SendAsync(data, WebSocketMessageType.Binary, endOfMessage,
                CancellationToken.None);
        }

        /// <summary>
        ///     Closes the WebSocket connection
        /// </summary>
        /// <param name="status"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public Task Close(
            WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
            string description = "")
        {
            return WebSocket.CloseAsync(status, description, CancellationToken.None);
        }

        internal async Task ReadFromWebSocket()
        {
            var buffer = ArrayPool<byte>.Shared.Rent(0x1000);
            try
            {
                var received =
                    await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
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
                            await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "",
                                CancellationToken.None);
                            break;
                    }

                    received = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer),
                        CancellationToken.None);
                }
            }
            catch (WebSocketException)
            {
            }
            finally
            {
                OnClosed?.Invoke(this, EventArgs.Empty);
                await WebSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "",
                    CancellationToken.None);
                WebSocket.Dispose();
            }
        }

        /// <summary>
        ///     Convenience method for ending a dialog handler
        /// </summary>
        /// <returns></returns>
        public HandlerType Final()
        {
            return HandlerType.Final;
        }

        /// <summary>
        ///     Convenience method for ending a dialog handler
        /// </summary>
        /// <returns></returns>
        public HandlerType Continue()
        {
            return HandlerType.Continue;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Represents a binary message received from websocket
        /// </summary>
        public sealed class BinaryMessageEventArgs : EventArgs
        {
            private readonly ArraySegment<byte> _arraySegment;

            /// <summary>
            ///     Whether this is a complete message or the end of one, or there is more to come.
            /// </summary>
            public readonly bool EndOfMessage;

            internal BinaryMessageEventArgs(ArraySegment<byte> data, bool endOfMessage)
            {
                _arraySegment = data;
                EndOfMessage = endOfMessage;
            }

            /// <summary>
            ///     The binary content of the message
            /// </summary>
            public byte[] Data => _arraySegment.Array;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Represents a UTF-8 encoded text message received from websocket
        /// </summary>
        public sealed class TextMessageEventArgs : EventArgs
        {
            /// <summary>
            ///     Whether this is a complete message or the end of one, or there is more to come.
            /// </summary>
            public readonly bool EndOfMessage;

            /// <summary>
            ///     The text content of the message
            /// </summary>
            public readonly string Text;

            internal TextMessageEventArgs(string text, bool endOfMessage)
            {
                Text = text;
                EndOfMessage = endOfMessage;
            }
        }
    }
}