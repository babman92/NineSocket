﻿using NineSocket.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NineSocket.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketServer server = new SocketServer(2015, 256, 10);            
            server.StartListening();
        }
    }
}