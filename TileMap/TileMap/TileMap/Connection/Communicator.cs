using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TileMap.Connection
{
    class Communicator
    {
        public void ConnectAsClient()
        {

            TcpClient client = new TcpClient();
            client.Connect(IPAddress.Parse("127.0.0.1"), 6000);
            NetworkStream stream = client.GetStream();
            String s = "JOIN#";
            byte[] message = Encoding.ASCII.GetBytes(s);
            stream.Write(message, 0, message.Length);
            //form.UpdateUI("Game inixialization Message Sent !");
            stream.Flush();
            stream.Close();
            client.Close();
        }
    }
}
