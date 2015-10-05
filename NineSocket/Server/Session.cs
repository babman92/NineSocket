using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NineSocket.Server
{
    public class Session
    {
        string sessionId;
        Socket client;
        public string SessionID { get { return sessionId; } }
        public Socket Client
        {
            get { return client; }
            private set
            {
                client = value;
                sessionId = client.RemoteEndPoint.ToString();
            }
        }
        public byte[] Buffer
        {
            get;
            set;
        }

        public int BufferSizeActual
        {
            get
            {
                return Buffer.Where(x => x != 0).Count();
            }
        }

        public Session(Socket _client, int bufferSize)
        {
            this.Client = _client;
            Buffer = new byte[bufferSize];
            sb = new StringBuilder();
        }

        public string Address
        {
            get
            {
                return client.RemoteEndPoint.ToString();
            }
        }

        public bool IsConnected
        {
            get
            {
                return !(client.Poll(1, SelectMode.SelectRead) && client.Available == 0 && client.Connected);
            }
        }

        public int GetByteRead(IAsyncResult ar)
        {
            return client.EndReceive(ar);
        }

        StringBuilder sb;

        public void AppendMessage(string message)
        {
            sb.Append(message);
        }

        public string Message
        {
            get { return sb.ToString(); }
        }

        public void ResetMesaage() { sb.Clear(); }

        public void BeginReceive(byte[] buffer, int offset, int bufferSize, SocketFlags socketFlag, AsyncCallback ReadCallback, Session session)
        {
            client.BeginReceive(session.Buffer, 0, bufferSize, socketFlag, new AsyncCallback(ReadCallback), session);
        }

        public void BeginSend(byte[] byteData, int offset, int bufferSize, SocketFlags socketFlag, AsyncCallback asyncCallback, Session session)
        {
            client.BeginSend(byteData, offset, bufferSize, socketFlag, asyncCallback, session);
        }

        public int EndSend(IAsyncResult ar)
        {
            return client.EndSend(ar);
        }

        public int EndReceive(IAsyncResult ar)
        {
            return client.EndReceive(ar);
        }
    }
}
