using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CommonLib;

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
                int primljeno = klijent.Receive(buffer);
                if (primljeno == 0) { klijent.Close(); lista.Remove(klijent); return; }

                string poruka = Encoding.UTF8.GetString(buffer, 0, primljeno);
                string odgovor = Procesuiraj(poruka);

                klijent.Send(Encoding.UTF8.GetBytes(odgovor));
            }
            catch { klijent.Close(); lista.Remove(klijent); }
        }

        static void ObradiUDP(Socket server)
        {
            byte[] buffer = new byte[1024];
            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            int primljeno = server.ReceiveFrom(buffer, ref remote);

            string poruka = Encoding.UTF8.GetString(buffer, 0, primljeno);
            string odgovor = Procesuiraj(poruka);

            server.SendTo(Encoding.UTF8.GetBytes(odgovor), remote);
        }

        static string Procesuiraj(string podaci)
        {
            string[] delovi = podaci.Split('|');
            if (delovi.Length < 3) return "Greska u formatu!";

            string algoritam = delovi[0];
            string kljuc = delovi[1];
            string tekst = delovi[2];

           /* if (algoritam == "Bajtovi")
                return SifrujBajtove(tekst, kljuc);
            if (algoritam == "Keyword")
                return SifrujKeyword(tekst, kljuc);
            if (algoritam == "Plejfer") 
                return "Plejfer: " + tekst.ToUpper(); */

            return "Nepoznat algoritam!";
        }

       /* static string SifrujBajtove(string tekst, string kljuc)
        {
            byte[] bTekst = Encoding.UTF8.GetBytes(tekst);
            byte bKljuc = (byte)(kljuc.Length > 0 ? kljuc[0] : 1);

            for (int i = 0; i < bTekst.Length; i++)
                bTekst[i] = (byte)(bTekst[i] ^ bKljuc);

            return Convert.ToBase64String(bTekst);
            return null;
        }
        public static string SifrujKeyword(string pocetna, string kljuc)
        {
            
            string abeceda = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string sifrovanaAbeceda = "";
            System.Collections.Generic.HashSet<char> iskoriscena = new System.Collections.Generic.HashSet<char>();

            
            foreach (char c in kljuc.ToUpper())
            {
                if (char.IsLetter(c) && !iskoriscena.Contains(c))
                {
                    sifrovanaAbeceda += c;
                    iskoriscena.Add(c);
                }
            }

            
            foreach (char c in abeceda)
            {
                if (!iskoriscena.Contains(c))
                {
                    sifrovanaAbeceda += c;
                }
            }

            System.Text.StringBuilder sifrovana = new System.Text.StringBuilder();

          
            foreach (char c in pocetna.ToUpper())
            {
                if (c >= 'A' && c <= 'Z')
                {
                    int index = c - 'A';
                    sifrovana.Append(sifrovanaAbeceda[index]);
                }
                else
                {
                    sifrovana.Append(c); 
                }
            }

            return sifrovana.ToString();
        }



        */

    }
}