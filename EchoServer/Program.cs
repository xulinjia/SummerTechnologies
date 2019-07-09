using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace EchoServer
{
    class ClientState
    {
        public Socket socket;
        public byte[] readBuff = new byte[1024];
    }

    class Program
    {
        static Socket listenfd;
        static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Bind
            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
            listenfd.Bind(ipEp);
            //Listen
            listenfd.Listen(0);
            Console.WriteLine("[服务器启动]");
            listenfd.BeginAccept(AcceptCallback, listenfd);
            Console.ReadLine();
        }

        static void AcceptCallback(IAsyncResult ar)
        {
            Console.WriteLine("服务器 Accept");
            Socket listenfd = (Socket)ar.AsyncState;
            Socket clientfd = listenfd.EndAccept(ar);
            ClientState state = new ClientState();
            state.socket = clientfd;
            clients.Add(clientfd, state);
            clientfd.BeginReceive(state.readBuff, 0, 1024, 0, ReceiveCallback, state);
            listenfd.BeginAccept(AcceptCallback, listenfd);
        }

        static void ReceiveCallback(IAsyncResult ar)
        {

            ClientState state = (ClientState)ar.AsyncState;
            Socket clientfd = state.socket;
            int count = clientfd.EndReceive(ar);
            if (count == 0)
            {
                clientfd.Close();
                clients.Remove(state.socket);
                Console.WriteLine("SocketClose");
            }

                 
            string recvStr = System.Text.Encoding.Default.GetString(state.readBuff, 0, count);
            Console.WriteLine("[服务器接收]" + recvStr);

            //Send 
            string sendStr = System.DateTime.Now.ToString();
            byte[] sendByte = System.Text.Encoding.Default.GetBytes(sendStr);
            clientfd.Send(sendByte);
            Console.WriteLine("[服务器发送]" + sendStr);
        }
    }
}
