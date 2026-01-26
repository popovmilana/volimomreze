using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 27015);

            Socket tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpServer.Bind(ipep);
            tcpServer.Listen(10);

            Socket udpServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpServer.Bind(ipep);


            List<Socket> sviSoketi = new List<Socket> { tcpServer, udpServer };

            Console.WriteLine("Server pokrenut na portu 27015...");

            while (true)
            {

                List<Socket> readList = new List<Socket>(sviSoketi);


                Socket.Select(readList, null, null, 1000);

                foreach (Socket s in readList)
                {
                    if (s == tcpServer)
                    {

                        Socket klijent = s.Accept();
                        sviSoketi.Add(klijent);
                        Console.WriteLine("Novi TCP klijent povezan.");
                    }
                    else if (s == udpServer)
                    {

                        ObradiUDP(s);
                    }
                    else
                    {
                        ObradiTCP(s, sviSoketi);
                    }
                }
            }
        }

        static void ObradiTCP(Socket klijent, List<Socket> lista)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int rec = klijent.Receive(buffer);
                if (rec == 0) { klijent.Close(); lista.Remove(klijent); return; }

                Console.WriteLine("Stigla TCP poruka.");
            }
            catch { klijent.Close(); lista.Remove(klijent); }
        }

        static void ObradiUDP(Socket server)
        {
            byte[] buffer = new byte[1024];
            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            int rec = server.ReceiveFrom(buffer, ref remote);

            Console.WriteLine("Stigla UDP poruka.");
        }
    }
}