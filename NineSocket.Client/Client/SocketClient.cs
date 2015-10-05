using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using NineSocket.Client.ClientHelper;
using System.Timers;
using NineSocket.Data;
using NineSocket.Client.Config;

namespace NineSocket.Client
{
    public class SocketClient
    {
        //variable client's info-----------------------
        string host;
        int port;
        int bufferSize;
        IPHostEntry ipHostInfo;
        IPAddress address;
        IPEndPoint remoteEnpoint;
        Socket socket;
        Session session;
        Timer timer;
        //----------------------------------------------
        //variable event--------------------------------
        public event EventHandler OnConnectedServer;
        public event EventHandler OnDisConnectedServer;
        public event EventHandler OnReceiveMessage;
        public event EventHandler OnSentMessage;
        public event EventHandler OnConnectFail;
        //end event-------------------------------------

        public SocketClient(string _host, int _port, int _bufferSize)
        {
            host = _host;
            port = _port;
            bufferSize = _bufferSize;
            ipHostInfo = Dns.Resolve(host);
            address = ipHostInfo.AddressList[0];
            remoteEnpoint = new IPEndPoint(address, port);
        }

        public SocketClient(int _port, int _bufferSize)
        {
            port = _port;
            bufferSize = _bufferSize;
            ipHostInfo = Dns.Resolve(Dns.GetHostName());
            address = ipHostInfo.AddressList[0];
            remoteEnpoint = new IPEndPoint(address, port);
        }

        public void StartClient()
        {
            timer = new Timer(10);
            timer.Elapsed += timer_Elapsed;

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.BeginConnect(remoteEnpoint, new AsyncCallback(ConnectCallBack), socket);
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (session != null)
                Receive(session);
        }

        private void Receive(Session session)
        {
            if (session.IsConnected)
                session.BeginReceive(session.Buffer, 0, bufferSize, 0,
                    new AsyncCallback(ReceiveCallback), session);
            else
            {
                timer.Stop();
                UtilityHelper.RaiseEvent(OnDisConnectedServer, session);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            string content = string.Empty;
            var session = (Session)ar.AsyncState;
            if (session.IsConnected)
            {
                int byteRead = -1;
                byteRead = session.BufferSizeActual;
                if (byteRead > 0)
                {
                    content = Encoding.UTF8.GetString(session.Buffer);
                    session.AppendMessage(content);
                    UtilityHelper.RaiseEvent(OnReceiveMessage, session.Message);
                    session.ResetMesaage();
                }
            }
        }

        private void ConnectCallBack(IAsyncResult ar)
        {
            socket = (Socket)ar.AsyncState;
            session = new Session(socket, bufferSize);
            if (session.IsConnected)
            {
                timer.Start();
                Console.WriteLine("Connected to server at {0}", socket.RemoteEndPoint.ToString());
                Send("hello server!!");
                UtilityHelper.RaiseEvent(OnConnectedServer, session);
            }
            else
            {
                UtilityHelper.RaiseEvent(OnConnectFail, new NineResult(Constant.CODE_CONNECT_TO_SERVER_FAIL.ToString(), "Error", "Connect to server failed!!!"));
            }
        }

        public void Send(String data)
        {
            //data = data + "<EOF>";
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            session.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), session);

            Receive(session);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var session = (Session)ar.AsyncState;
                int bytesSent = session.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);
                UtilityHelper.RaiseEvent(OnSentMessage, session);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
