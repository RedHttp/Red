using System;
using System.Buffers;
using System.Linq;
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

        private readonly CancellationToken _requestAborted;

        internal WebSocketDialog(WebSocket webSocket, CancellationToken requestAborted)
        {
            WebSocket = webSocket;
            _requestAborted = requestAborted;
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
                WebSocketMessageType.Text, endOfMessage, _requestAborted);
        }

        /// <summary>
        ///     Send binary message using websocket
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endOfMessage"></param>
        public Task SendBytes(ArraySegment<byte> data, bool endOfMessage = true)
        {
            return WebSocket.SendAsync(data, WebSocketMessageType.Binary, endOfMessage, _requestAborted);
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
            return WebSocket.CloseAsync(status, description, _requestAborted);
        }

        internal async Task ReadFromWebSocket()
        {
            var buffer = ArrayPool<byte>.Shared.Rent(0x1000);
            try
            {
                var received = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _requestAborted);
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
                            await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", _requestAborted);
                            break;
                    }

                    received = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _requestAborted);
                }
            }
            catch (WebSocketException)
            {
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                try
                {
                    if (!new [] { WebSocketState.Aborted, WebSocketState.Closed}.Contains(WebSocket.State))
                    {
                        await WebSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "", _requestAborted);
                    }
                }
                catch (WebSocketException) { }
                
                OnClosed?.Invoke(this, EventArgs.Empty);
                WebSocket.Dispose();
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Represents a binary message received from websocket
        /// </summary>
        public sealed class BinaryMessageEventArgs : EventArgs
        {
            /// <summary>
            ///     Whether this is a complete message or the end of one, or there is more to come.
            /// </summary>
            public readonly bool EndOfMessage;

            /// <summary>
            ///     The binary content of the message
            /// </summary>
            public readonly ArraySegment<byte> Data;

            internal BinaryMessageEventArgs(ArraySegment<byte> data, bool endOfMessage)
            {
                Data = data;
                EndOfMessage = endOfMessage;
            }
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