using NineSocket.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NineSocket.Client.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var socketClient = new SocketClient(2015, 256);
            socketClient.OnConnectedServer += socketClient_OnConnectedServer;
            socketClient.OnReceiveMessage += socketClient_OnReceiveMessage;
            socketClient.OnDisConnectedServer += socketClient_OnDisConnectedServer;
            socketClient.OnConnectFail += socketClient_OnConnectFail;
            socketClient.StartClient();

            Console.ReadLine();
        }

        static void socketClient_OnConnectFail(object sender, EventArgs e)
        {
            Console.WriteLine((sender as NineResult).GetData("Error"));
        }

        static void socketClient_OnDisConnectedServer(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnect from {0}", (sender as Session).Address);
        }

        static void socketClient_OnReceiveMessage(object sender, EventArgs e)
        {
            Console.WriteLine(sender.ToString());
        }

        static void socketClient_OnConnectedServer(object sender, EventArgs e)
        {

        }
    }
}
