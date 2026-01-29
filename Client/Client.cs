using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CommonLib;

namespace Client
{
    internal class Client
    {
        static void Main(string[] args)
        {

            Console.WriteLine("====================== KLIJENTSKA APLIKACIJA ======================");
            //Client klijent = new Client();
            Socket klijentSoket = null;
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse("192.168.56.1"), 27015);

            string izborProtokola;
            Console.WriteLine("\nIzabrati protokol: ");
            Console.WriteLine("1. UDP");
            Console.WriteLine("2. TCP");
            Console.Write("Izabor: ");
            izborProtokola = Console.ReadLine();

            if (izborProtokola == "1")//UDP
            {
                Console.Clear();
                Console.WriteLine("UDP protokol izabran...");

                klijentSoket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            }
            else if (izborProtokola == "2")//TCP
            {
                Console.Clear();
                Console.WriteLine("TCP protokol izabran...");
                klijentSoket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    klijentSoket.Connect(serverEP);
                    Console.WriteLine("\nPovezan sa serverom.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Greška pri povezivanju: " + ex.Message);
                    Console.ReadKey();
                    return;
                }
            }
            else
            {
                Console.WriteLine("pogresan unos...");
                Console.ReadKey();
                return;
            }

            while (true)
            {
                Console.WriteLine("\n" + new string('-', 50));
                string izborSifrovanja;
                string algoritam = "";
                Console.WriteLine("Izabrati algoritam šifrovanja:");
                Console.WriteLine("1. Sifrovanje upotrebom bitova (XOR)");
                Console.WriteLine("2. Plejfer algoritam");
                Console.WriteLine("3. Keyword");
                Console.WriteLine("0. Izlaz");
                Console.Write("Izbor: ");
                izborSifrovanja = Console.ReadLine();

                if(izborSifrovanja == "0")
                    break; 

                Console.Write("Unesite tekst poruke: ");
                string tekst = Console.ReadLine();
                Console.Write("Unesite kljuc: ");
                string kljuc = Console.ReadLine();
                string sifrovanaPoruka = "";

                if (izborSifrovanja == "1")
                {
                    Console.WriteLine("Sifrovanje upotrebom bajtova izabrano...");
                    algoritam = "Bajtovi";
                    BitoviAlgoritam alg = new BitoviAlgoritam(tekst, kljuc);
                    sifrovanaPoruka = alg.Enkriptuj();
                    Console.WriteLine("[SIFROVANO]: "+sifrovanaPoruka);
                }
                else if (izborSifrovanja == "2")
                {
                    Console.WriteLine("Plejfer sifrovanje izabrano...");
                    algoritam = "Plejfer";
                    PlejferAlgoritam alg = new PlejferAlgoritam(tekst, kljuc);
                    sifrovanaPoruka = alg.Enkriptuj();
                    Console.WriteLine("[SIFROVANO]: " + sifrovanaPoruka);
                }
                else if (izborSifrovanja == "3")
                {
                    Console.WriteLine("Keyword sifrovanje izabrano...");
                    algoritam = "Keyword";
                    KeywordAlgoritam alg = new KeywordAlgoritam(tekst, kljuc);
                    sifrovanaPoruka = alg.Enkriptuj();
                    Console.WriteLine("[SIFROVANO]: " + sifrovanaPoruka);
                }
                else
                {
                    Console.WriteLine("pogresan unos...");
                    continue;
                }

                string sifrovanaPorukaZaSlanje = $"{algoritam}|{kljuc}|{sifrovanaPoruka}";
                byte[] podaci = Encoding.UTF8.GetBytes(sifrovanaPorukaZaSlanje);


                //odgovor od servera
                try
                {
                    if (izborProtokola == "1") // UDP
                        klijentSoket.SendTo(podaci, serverEP);
                    else // TCP
                        klijentSoket.Send(podaci);
                    Console.WriteLine("Poruka poslata serveru...");

                    byte[] buffer = new byte[2024];
                    int primljeno;

                    if (izborProtokola == "1")//UDP
                    {
                        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                        primljeno = klijentSoket.ReceiveFrom(buffer, ref remoteEP);
                    }
                    else//TCP
                    {
                        primljeno = klijentSoket.Receive(buffer);
                    }

                    string sifrovanOdgovor = Encoding.UTF8.GetString(buffer, 0, primljeno);
                    Console.WriteLine("\n[SERVER ODGOVOR]:" + sifrovanOdgovor + "\n");


                    //desifrovanje odgovora
                    string desifrovaniOdgovor = "";
                    if(algoritam == "Bajtovi")
                    {
                        BitoviAlgoritam alg = new BitoviAlgoritam(sifrovanOdgovor, kljuc);
                        desifrovaniOdgovor = alg.Dekriptuj();
                    }
                    else if(algoritam == "Plejfer")
                    {
                        PlejferAlgoritam alg = new PlejferAlgoritam(sifrovanOdgovor, kljuc);
                        desifrovaniOdgovor = alg.Dekriptuj();
                    }
                    else if(algoritam == "Keyword")
                    {
                        KeywordAlgoritam alg = new KeywordAlgoritam(sifrovanOdgovor, kljuc);
                        desifrovaniOdgovor = alg.Dekriptuj();
                    }
                    else
                    {
                        Console.WriteLine("[GRESKA]: Greska prilikom desifrovanja odgovora od servera....");
                    }

                        Console.WriteLine("[DESIFROVAN ODGOVOR]: " + desifrovaniOdgovor);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Greska u komunikaciji: " + ex.Message);
                    break;
                }
            }

            klijentSoket.Close();
            Console.WriteLine("Klijent zavrsava sa radom...\nPritisnite bilo koji taster za kraj rada...");
            Console.ReadKey();
        }



    }
}
