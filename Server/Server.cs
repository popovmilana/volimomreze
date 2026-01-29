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
        static Dictionary<Socket, NacinKomunikacije> klijenti = new Dictionary<Socket, NacinKomunikacije>();
        static Dictionary<string, NacinKomunikacije> udpKlijenti = new Dictionary<string, NacinKomunikacije>();

        static void Main(string[] args)
        {
            Console.WriteLine("====================== SERVER APLIKACIJA ======================");
            Console.WriteLine("\nIzaberite protokol:");
            Console.WriteLine("1. UDP");
            Console.WriteLine("2. TCP");
            Console.Write("Izbor: ");
            string izborProtokola = Console.ReadLine();

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 27015);

            if (izborProtokola == "2") // TCP
            {
                PokreciTCPServer(ipep);
            }
            else if (izborProtokola == "1") // UDP
            {
                PokreciUDPServer(ipep);
            }
            else
            {
                Console.WriteLine("Neispravan izbor!");
                Console.ReadKey();
            }
        }

        static void PokreciTCPServer(IPEndPoint ipep)
        {
            Socket tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpServer.Bind(ipep);
            tcpServer.Listen(10);

            // Dobij stvarnu IP adresu
            string ipAdresa = GetLocalIPAddress();
            Console.WriteLine("\n=== TCP SERVER POKRENUT ===");
            Console.WriteLine($"IP Adresa: {ipAdresa}");
            Console.WriteLine($"Port: {ipep.Port}");
            Console.WriteLine("Čekam klijente...\n");

            List<Socket> sviSoketi = new List<Socket> { tcpServer };

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
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Novi klijent povezan.");
                    }
                    else
                    {
                        ObradiTCP(s, sviSoketi);
                    }
                }
            }
        }

        static void PokreciUDPServer(IPEndPoint ipep)
        {
            Socket udpServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpServer.Bind(ipep);

            string ipAdresa = GetLocalIPAddress();
            Console.WriteLine("\n=== UDP SERVER POKRENUT ===");
            Console.WriteLine($"IP Adresa: {ipAdresa}");
            Console.WriteLine($"Port: {ipep.Port}");
            Console.WriteLine("Čekam poruke...\n");

            List<Socket> sviSoketi = new List<Socket> { udpServer };

            while (true)
            {
                // Koristi Select za multipleksiranje (zadatak 7)
                List<Socket> readList = new List<Socket>(sviSoketi);
                Socket.Select(readList, null, null, 1000);

                foreach (Socket s in readList)
                {
                    ObradiUDP(s);
                }
            }
        }

        static void ObradiTCP(Socket klijent, List<Socket> lista)
        {
            try
            {
                byte[] buffer = new byte[2048];
                int primljeno = klijent.Receive(buffer);

                if (primljeno == 0)
                {
                    if (klijenti.ContainsKey(klijent))
                        klijenti.Remove(klijent);
                    klijent.Close();
                    lista.Remove(klijent);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Klijent odspojen.");
                    return;
                }

                string poruka = Encoding.UTF8.GetString(buffer, 0, primljeno);
                //string odgovor = Procesuiraj(poruka, klijent, null);
                string odgovor = Procesuiraj(poruka, klijent, null);

                klijent.Send(Encoding.UTF8.GetBytes(odgovor));
                Console.WriteLine("[STATUS] Odgovor poslat klijentu.\n");
            }
            catch
            {
                if (klijenti.ContainsKey(klijent))
                    klijenti.Remove(klijent);
                klijent.Close();
                lista.Remove(klijent);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Greška - klijent uklonjen.");
            }
        }

        static void ObradiUDP(Socket server)
        {
            byte[] buffer = new byte[2048];
            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            int primljeno = server.ReceiveFrom(buffer, ref remote);

            string poruka = Encoding.UTF8.GetString(buffer, 0, primljeno);
            string odgovor = Procesuiraj(poruka, null, remote);

            server.SendTo(Encoding.UTF8.GetBytes(odgovor), remote);
        }

        static string Procesuiraj(string podaci, Socket tcpKlijent, EndPoint udpKlijent)
        {
            string[] delovi = podaci.Split('|');
            if (delovi.Length < 3)
                return "Greška u formatu poruke!";

            string algoritam = delovi[0];
            string kljuc = delovi[1];
            string sifrovanaPoruka = delovi[2];

            // Čuvaj/ažuriraj informacije o klijentu
            NacinKomunikacije nacinKom = new NacinKomunikacije
            {
                algoritam = algoritam,
                kljuc = kljuc,
                poruka = sifrovanaPoruka
            };

            if (tcpKlijent != null)
            {
                klijenti[tcpKlijent] = nacinKom;
            }
            else if (udpKlijent != null)
            {
                udpKlijenti[udpKlijent.ToString()] = nacinKom;
            }
            /*
                        if (tcpKlijent != null)
                        {
                            if (!klijenti.ContainsKey(tcpKlijent))
                                klijenti[tcpKlijent] = nacinKom;
                            else
                            {
                                klijenti[tcpKlijent].algoritam = algoritam;
                                klijenti[tcpKlijent].kljuc = kljuc;
                                klijenti[tcpKlijent].poruka = sifrovanaPoruka;
                            }
                        }
                        else if (udpKlijent != null)
                        {
                            string kljucKlijenta = udpKlijent.ToString();
                            udpKlijenti[kljucKlijenta] = nacinKom;
                        }
            */
            // Dešifruj primljenu poruku
            string desifrovano = Desifruj(sifrovanaPoruka, algoritam, kljuc);
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] STIGLA PORUKA OD KLIJENTA");
            Console.WriteLine($"Algoritam: {algoritam}");
            Console.WriteLine($"Sadržaj(desifrovana): {desifrovano}");
            Console.WriteLine("------------------------------------------------");
            // Kreiraj odgovor
            Console.Write("Unesite odgovor za klijenta: ");
            string odgovorTekst = Console.ReadLine();

            // Šifruj odgovor istim algoritmom
            string sifrovaniOdgovor = Sifruj(odgovorTekst, algoritam, kljuc);

            return sifrovaniOdgovor;
        }

        static string Desifruj(string sifrovano, string algoritam, string kljuc)
        {
            try
            {
                if (algoritam == "Bajtovi")
                {
                    BitoviAlgoritam alg = new BitoviAlgoritam(sifrovano, kljuc);
                    return alg.Dekriptuj();
                }
               /* else if (algoritam == "Plejfer")
                {
                    PlejferAlgoritam alg = new PlejferAlgoritam(sifrovano, kljuc);
                    return alg.Dekriptuj();
                }*/
                else if (algoritam == "Keyword")
                {
                    KeywordAlgoritam alg = new KeywordAlgoritam(sifrovano, kljuc);
                    return alg.Dekriptuj();
                }
                return "Nepoznat algoritam!";
            }
            catch (Exception ex)
            {
                return $"Greška pri dešifrovanju: {ex.Message}";
            }
        }

        static string Sifruj(string tekst, string algoritam, string kljuc)
        {
            try
            {
                if (algoritam == "Bajtovi")
                {
                    BitoviAlgoritam alg = new BitoviAlgoritam(tekst, kljuc);
                    return alg.Enkriptuj();
                }
                /*else if (algoritam == "Plejfer")
                {
                    PlejferAlgoritam alg = new PlejferAlgoritam(tekst, kljuc);
                    return alg.Enkriptuj();
                }*/
                else if (algoritam == "Keyword")
                {
                    KeywordAlgoritam alg = new KeywordAlgoritam(tekst, kljuc);
                    return alg.Enkriptuj();
                }
                return "Nepoznat algoritam!";
            }
            catch (Exception ex)
            {
                return $"Greška pri šifrovanju: {ex.Message}";
            }
           }

        static string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }
    }
}