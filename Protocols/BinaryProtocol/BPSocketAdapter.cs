using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketJSON;

namespace BinaryProtocol
{
    /// <summary>
    /// This is a basic implementation of the ISocket used by the WSJConnection as a raw socket. Internally it uses
    /// TcpSocket for communication.
    /// </summary>
    public class BPSocketAdapter : ISocket
    {
        /// <summary>
        /// Creates a socket by establishing a connection to the remote server.
        /// </summary>
        /// <param name="aHost">Remote host name.</param>
        /// <param name="aPort">Remote port.</param>
        public BPSocketAdapter(string aHost, int aPort)
        {
            host = aHost;
            port = aPort;
        }

        /// <summary>
        /// Creates a socket based on the existing TcpClient. When this constructor is used, Opened event will not be
        /// fired and provided client is expected to be already connected. Calling Open will result in an error.
        /// </summary>
        /// <param name="connectedClient"></param>
        public BPSocketAdapter(TcpClient connectedClient)
        {
            isOpened = 1;
            client = connectedClient;
            SetUpBufferAndStartReading();
        }

        public event EventHandler Opened;

        public event EventHandler Closed;

        public event EventHandler<WebSocketJSON.SocketErrorEventArgs> Error;

        public event EventHandler<MessageEventArgs> Message;

        public void Open()
        {
            client = new TcpClient();
            client.BeginConnect(host, port, HandleConnected, null);
        }

        public void Close()
        {
            client.Close();
        }

        public void Send(string message)
        {
            if (client.Connected)
            {
                try
                {
                    // Each message is preceded by the 4-byte length and the content is encoded using UTF8.
                    byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
                    byte[] lenBytes = BitConverter.GetBytes(messageBytes.Length);

                    byte[] bytes = new byte[lenBytes.Length + messageBytes.Length];
                    Buffer.BlockCopy(lenBytes, 0, bytes, 0, lenBytes.Length);
                    Buffer.BlockCopy(messageBytes, 0, bytes, lenBytes.Length, messageBytes.Length);

                    stream.BeginWrite(bytes, 0, bytes.Length, HandleWriteFinished, null);
                }
                catch (IOException e)
                {
                    HandleError(e);
                }
            }
            else
            {
                if (!HandleNotConnected())
                {
                    HandleError(new IOException(
                        "Failed to send. Socket is not opened yet or have been closed already."));
                }
            }
        }

        void HandleWriteFinished(IAsyncResult ar)
        {
            try
            {
                stream.EndWrite(ar);
            }
            catch (IOException)
            {
                // If the socket has closed before the write completed, EndWrite will throw. We just ignore this.
            }
        }

        public bool IsConnected
        {
            get
            {
                return client.Connected;
            }
        }

        /// <summary>
        /// Thread-safe method for raising Closed event when the connection is closed. If the connection was closed
        /// before this method was called, the Closed event is not raised and false is returned.
        /// </summary>
        /// <returns>True if the connection has been closed before this method was called.</returns>
        private bool HandleNotConnected()
        {
            int wasOpened = Interlocked.CompareExchange(ref isOpened, 0, 1);
            if (wasOpened == 1)
            {
                if (Closed != null)
                    Closed(this, new EventArgs());
                return true;
            }

            return false;
        }

        /// <summary>
        /// Raises Error event with a given exception.
        /// </summary>
        /// <param name="exception">An exception that is passed with event arguments.</param>
        private void HandleError(Exception exception)
        {
            if (Error != null)
                Error(this, new SocketErrorEventArgs(exception));
        }

        /// <summary>
        /// Handles opened connection, starts listening for messages and raises Opened event.
        /// </summary>
        /// <param name="ar"></param>
        private void HandleConnected(IAsyncResult ar)
        {
            client.EndConnect(ar);

            isOpened = 1;

            SetUpBufferAndStartReading();

            if (Opened != null)
                Opened(this, new EventArgs());
        }

        /// <summary>
        /// Creates necessary buffers and starts listening for the first message.
        /// </summary>
        private void SetUpBufferAndStartReading()
        {
            receiveBuffer = new byte[client.ReceiveBufferSize];
            stream = client.GetStream();
            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, HandleFinishedRead, null);
        }

        /// <summary>
        /// Asynchronous handler for the new data arriving from the socket. Processes the new data, extracts all
        /// completed messages and raises Message event for each of them.
        /// </summary>
        /// <param name="ar">Asynchronous result.</param>
        private void HandleFinishedRead(IAsyncResult ar)
        {
            if (!IsConnected)
            {
                if (!HandleNotConnected())
                {
                    HandleError(new IOException(
                        "Can not read from socket that has been already closed or not yet opened."));
                }
                return;
            }

            try
            {
                int readBytes = stream.EndRead(ar);
                int offset = 0;

                while (offset < readBytes)
                {
                    int availableBytes = readBytes - offset;
                    if (incompleteMessage == null)
                    {
                        if (incompleteMessageSize == null)
                            InitializeMessageSize();

                        if (availableBytes + messageSizeCompletedBytes >= incompleteMessageSize.Length)
                            ReadCompleteMessageSize(ref offset);
                        else
                            ReadPartialMessageSize(ref offset, availableBytes);
                    }
                    else
                    {
                        if (availableBytes + messageCompletedBytes >= incompleteMessage.Length)
                            ReadCompleteMessage(ref offset);
                        else
                            ReadPartialMessage(ref offset, availableBytes);
                    }
                }

                stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, HandleFinishedRead, null);
            }
            catch (IOException e)
            {
                HandleError(e);
            }
        }

        /// <summary>
        /// Reads part of the message from the buffer.
        /// </summary>
        /// <param name="offset">Position at the buffer where reading should start.</param>
        /// <param name="availableBytes">Number of bytes that are available.</param>
        /// <returns>New offset.</returns>
        private void ReadPartialMessage(ref int offset, int availableBytes)
        {
            Buffer.BlockCopy(receiveBuffer, offset, incompleteMessage, messageCompletedBytes,
                availableBytes);
            messageCompletedBytes += availableBytes;
            offset += availableBytes;
        }

        /// <summary>
        /// Reads completed message from the buffer.
        /// </summary>
        /// <param name="offset">Position at the buffer where reading should start.</param>
        /// <returns>New offset.</returns>
        private void ReadCompleteMessage(ref int offset)
        {
            Buffer.BlockCopy(receiveBuffer, offset, incompleteMessage, messageCompletedBytes,
                incompleteMessage.Length - messageCompletedBytes);
            offset += incompleteMessage.Length - messageCompletedBytes;
            string message = System.Text.Encoding.UTF8.GetString(incompleteMessage);
            if (Message != null)
                Message(this, new MessageEventArgs(message));
            incompleteMessage = null;
        }

        /// <summary>
        /// Reads part of the message size from the buffer.
        /// </summary>
        /// <param name="offset">Position at the buffer where reading should start.</param>
        /// <param name="availableBytes">Number of bytes that are available.</param>
        /// <returns>New offset.</returns>
        private void ReadPartialMessageSize(ref int offset, int availableBytes)
        {
            Buffer.BlockCopy(receiveBuffer, offset, incompleteMessageSize, messageSizeCompletedBytes,
                availableBytes);
            messageSizeCompletedBytes += availableBytes;
            offset += availableBytes;
        }

        /// <summary>
        /// Reads completed message size from the buffer.
        /// </summary>
        /// <param name="offset">Position at the buffer where reading should start.</param>
        /// <returns>New offset.</returns>
        private void ReadCompleteMessageSize(ref int offset)
        {
            Buffer.BlockCopy(receiveBuffer, offset, incompleteMessageSize, messageSizeCompletedBytes,
                incompleteMessageSize.Length - messageSizeCompletedBytes);
            offset += incompleteMessageSize.Length - messageSizeCompletedBytes;
            int messageSize = BitConverter.ToInt32(incompleteMessageSize, 0);
            incompleteMessageSize = null;
            incompleteMessage = new byte[messageSize];
            messageCompletedBytes = 0;
        }

        private void InitializeMessageSize()
        {
            incompleteMessageSize = new byte[sizeof(int)];
            messageSizeCompletedBytes = 0;
        }

        private byte[] receiveBuffer;
        private byte[] incompleteMessage = null;
        private byte[] incompleteMessageSize = null;
        int messageCompletedBytes;
        int messageSizeCompletedBytes;

        private TcpClient client;
        private NetworkStream stream;

        int isOpened = 0;  // we use int to be able to use Interlocked.CompareExchange. value 0 means false, 1 - true

        string host;
        int port;
    }
}
