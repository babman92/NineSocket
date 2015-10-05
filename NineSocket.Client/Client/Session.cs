using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NineSocket.Client
{
    public class Session
    {
        public byte[] Buffer { get; set; }
        Socket client;
        StringBuilder sb;
        public Session(Socket _client, int _bufferSize)
        {
            sb = new StringBuilder();
            client = _client;
            Buffer = new byte[_bufferSize];
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
                return (!(client.Poll(1, SelectMode.SelectRead) && client.Available == 0) && client.Connected);
            }
        }

        public int BufferSizeActual
        {
            get
            {
                return Buffer.Where(x => x != 0).Count();
            }
        }

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
    }
}
