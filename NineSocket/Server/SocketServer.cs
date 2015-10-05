using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;
using NineSocket.ServerHelper;
namespace NineSocket.Server
{
    public class SocketServer
    {
        //variable server specified-------
        Socket listener;
        int bufferSize;
        string host;
        int port;
        int backlog;
        IPHostEntry ipHostInfo;
        IPAddress ipAddress;
        IPEndPoint localEndPoint;
        public ManualResetEvent allDone = new ManualResetEvent(false);
        //end server specified-------------

        //variable event-------------------
        public event EventHandler OnNewClientConnected;
        public event EventHandler OnNewClientRequest;
        //public event 

        //end event------------------------

        /// <summary>
        /// Contructor init server's info
        /// </summary>
        /// <param name="_host">Host name or Ip adress</param>
        /// <param name="_port">Port of app server</param>
        /// <param name="_bufferSize">Size of buffer send, receive</param>
        /// <param name="_backlog">Number pending connection queue</param>
        public SocketServer(string _host, int _port, int _bufferSize, int _backlog)
        {
            backlog = _backlog;
            host = _host;
            port = _port;
            bufferSize = _bufferSize;
            ipHostInfo = Dns.Resolve(host);
            ipAddress = ipHostInfo.AddressList[0];
            localEndPoint = new IPEndPoint(ipAddress, port);
        }

        /// <summary>
        /// Contructor init server's info, Host name is Server's address (default)
        /// </summary>
        /// <param name="_port">Port of app server</param>
        /// <param name="_bufferSize">Size of buffer send, receive</param>
        /// <param name="_backlog">Number pending connection queue</param>
        public SocketServer(int _port, int _buffeerSize, int _backlog)
        {
            backlog = _backlog;
            port = _port;
            bufferSize = _buffeerSize;
            ipHostInfo = Dns.Resolve(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList[0];
            localEndPoint = new IPEndPoint(ipAddress, port);
        }

        /// <summary>
        /// Server start listen connection from client
        /// </summary>
        public void StartListening()
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(backlog);
                Console.WriteLine("Waiting for a connection at ip {0} and port {1} ...", localEndPoint.Address.ToString(), localEndPoint.Port);
                while (true)
                {
                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallBack), listener);
                    allDone.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void AcceptCallBack(IAsyncResult ar)
        {
            allDone.Set();
            listener = (Socket)ar.AsyncState;
            var handle = listener.EndAccept(ar);
            //handle.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            var session = new Session(handle, bufferSize);
            Console.WriteLine("{0} connected at {1}", session.SessionID, session.Address);
            //Send(session, "hello client, welcome you!!!!!!!");
            UtilityHelper.RaiseEvent(OnNewClientConnected, session);
            session.BeginReceive(session.Buffer, 0, bufferSize, 0, new AsyncCallback(ReadCallback), session);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            Task.Run(() =>
            {
                string content = string.Empty;
                var session = (Session)ar.AsyncState;
                if (session.IsConnected)
                {
                    int byteRead = -1;
                    //byteRead = session.BufferSizeActual;
                    byteRead = session.Client.EndReceive(ar);

                    if (byteRead > 0)
                    {
                        content = Encoding.UTF8.GetString(session.Buffer).TrimEnd('\0');
                        session.AppendMessage(content);
                        //finish receive
                        //if (content.EndsWith("\r\n"))
                        {
                            UtilityHelper.RaiseEvent(OnNewClientRequest, session.Message);
                            Console.WriteLine("{0} say: {1}", session.SessionID, session.Message);
                            session.ResetMesaage();
                        }
                        //else // continue receive
                        {
                            //session.BeginReceive(session.Buffer, 0, bufferSize, 0, new AsyncCallback(ReadCallback), session);
                        }
                    }
                }
                else
                {
                    //session disconnected
                    Console.WriteLine("{0} disconnected!", session.SessionID);
                }
            });
        }

        public void Send(Session session, string data)
        {
            Task.Run(() =>
            {
                byte[] byteData = Encoding.UTF8.GetBytes(data);

                // Begin sending the data to the remote device.
                session.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), session);
            });
        }

        private void SendCallback(IAsyncResult ar)
        {
            var session = (Session)ar.AsyncState;
            int bytesSent = session.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            session.BeginReceive(session.Buffer, 0, bufferSize, 0, new AsyncCallback(ReadCallback), session);
        }
    }
}
