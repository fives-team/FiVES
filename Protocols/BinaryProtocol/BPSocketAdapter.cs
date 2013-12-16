using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketJSON;

namespace BinaryProtocol
{
    public class BPSocketAdapter : ISocket
    {
        public BPSocketAdapter(string aHost, int aPort)
        {
            host = aHost;
            port = aPort;
        }

        public BPSocketAdapter(TcpClient connectedClient)
        {
            client = connectedClient;
            SetUpBufferAndStartReading();
        }

        public event EventHandler Opened;

        public event EventHandler Closed;

        public event EventHandler<ErrorEventArgs> Error;

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
                // Each message is preceded by the 4-byte length and the content is encoded using UTF8.
                byte[] msg = System.Text.Encoding.UTF8.GetBytes(message);
                byte[] msgLength = BitConverter.GetBytes(msg.Length);
                stream.Write(msgLength, 0, msgLength.Length);
                stream.Write(msg, 0, msg.Length);
            }
            else
            {
                HandleNotConnected();
            }
        }

        public bool IsConnected
        {
            get
            {
                return client.Connected;
            }
        }

        private void HandleNotConnected()
        {
            int wasOpened = Interlocked.CompareExchange(ref isOpened, 0, 1);
            if (wasOpened == 1)
            {
                if (Closed != null)
                    Closed(this, new EventArgs());
            }
            else
            {
                if (Error != null) 
                {
                    var e = new Exception("Failed to send. Socket is not yet opened or have been closed already");
                    Error(this, new ErrorEventArgs(e));
                }
            }
        }

        private void HandleConnected(IAsyncResult ar)
        {
            client.EndConnect(ar);

            SetUpBufferAndStartReading();

            if (Opened != null)
                Opened(this, new EventArgs());
        }

        private void SetUpBufferAndStartReading()
        {
            receiveBuffer = new byte[client.ReceiveBufferSize];
            stream = client.GetStream();
            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, HandleFinishedRead, null);
        }

        private void HandleFinishedRead(IAsyncResult ar)
        {
            if (!IsConnected)
            {
                HandleNotConnected();
                return;
            }

            int readBytes = stream.EndRead(ar);
            int offset = 0;

            while (offset < readBytes)
            {
                int availableBytes = readBytes - offset;
                if (incompleteMessage == null)
                {
                    if (incompleteMessageSize == null)
                    {
                        incompleteMessageSize = new byte[sizeof(int)];
                        messageSizeCompletedBytes = 0;
                    }

                    if (availableBytes + messageSizeCompletedBytes >= incompleteMessageSize.Length)
                    {
                        Buffer.BlockCopy(receiveBuffer, offset, incompleteMessageSize, messageSizeCompletedBytes,
                            incompleteMessageSize.Length - messageSizeCompletedBytes);
                        offset += incompleteMessageSize.Length - messageSizeCompletedBytes;
                        int messageSize = BitConverter.ToInt32(incompleteMessageSize, 0);
                        incompleteMessageSize = null;
                        incompleteMessage = new byte[messageSize];
                        messageCompletedBytes = 0;
                    }
                    else
                    {
                        Buffer.BlockCopy(receiveBuffer, offset, incompleteMessageSize, messageSizeCompletedBytes,
                            availableBytes);
                        messageSizeCompletedBytes += availableBytes;
                        offset += availableBytes;
                    }
                }
                else
                {
                    if (availableBytes + messageCompletedBytes >= incompleteMessage.Length)
                    {
                        Buffer.BlockCopy(receiveBuffer, offset, incompleteMessage, messageCompletedBytes,
                            incompleteMessage.Length - messageCompletedBytes);
                        offset += incompleteMessage.Length - messageCompletedBytes;
                        string message = System.Text.Encoding.UTF8.GetString(incompleteMessage);
                        if (Message != null)
                            Message(this, new MessageEventArgs(message));
                        incompleteMessage = null;
                    }
                    else
                    {
                        Buffer.BlockCopy(receiveBuffer, offset, incompleteMessage, messageCompletedBytes,
                            availableBytes);
                        messageCompletedBytes += availableBytes;
                        offset += availableBytes;
                    }
                }
            }

            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, HandleFinishedRead, null);
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
