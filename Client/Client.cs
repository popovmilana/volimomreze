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

            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Clear();

            Console.WriteLine("====================== KLIJENTSKA APLIKACIJA ======================");
            //Client klijent = new Client();
            Socket klijentSoket = null;
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse("192.168.56.1"), 27015);

            string izborProtokola;
            Console.WriteLine("\nIzabrati protokol: ");
            Console.WriteLine(" 1. UDP");
            Console.WriteLine(" 2. TCP");
            Console.Write("[IZBOR] ->  ");
            izborProtokola = Console.ReadLine();

            if (izborProtokola == "1")//UDP
            {
                //Console.Clear();
                Console.WriteLine("\n--------------------UDP SERVER POKRENUT --------------------");

                klijentSoket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            }
            else if (izborProtokola == "2")//TCP
            {
                //Console.Clear();
                Console.WriteLine("\n--------------------TCP SERVER POKRENUT --------------------");
                klijentSoket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    klijentSoket.Connect(serverEP);
                    Console.WriteLine("\nPovezan sa serverom.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[Greška pri povezivanju]: " + ex.Message);
                    Console.ReadKey();
                    return;
                }
            }
            else
            {
                Console.WriteLine("Pogresan unos.");
                Console.ReadKey();
                return;
            }

            while (true)
            {
                //Console.WriteLine("\n" + new string('-', 60));
                string izborSifrovanja;
                string algoritam = "";
                Console.WriteLine("Izabrati algoritam šifrovanja:");
                Console.WriteLine(" 1. Sifrovanje upotrebom bitova (XOR)");
                Console.WriteLine(" 2. Plejfer algoritam");
                Console.WriteLine(" 3. Keyword");
                Console.WriteLine(" 0. Izlaz");
                Console.Write("[IZBOR] -> ");
                izborSifrovanja = Console.ReadLine();

                if(izborSifrovanja == "0")
                    break; 

                Console.Write("[UNOS TEKSTA ] -> ");
                string tekst = Console.ReadLine();
                Console.Write("[UNOS KLJUCA ] -> ");
                string kljuc = Console.ReadLine();
                Console.WriteLine();
                string sifrovanaPoruka = "";

                if (izborSifrovanja == "1")
                {
                    Console.WriteLine("[IZABRANO SIFROVANJE]: Sifrovanje upotrebom bajtova");
                    algoritam = "Bajtovi";
                    BitoviAlgoritam alg = new BitoviAlgoritam(tekst, kljuc);
                    sifrovanaPoruka = alg.Enkriptuj();
                    Console.WriteLine("[SIFROVANO]: "+sifrovanaPoruka);
                }
                else if (izborSifrovanja == "2")
                {
                    Console.WriteLine("[IZABRANO SIFROVANJE]: Plejfer");
                    algoritam = "Plejfer";
                    PlejferAlgoritam alg = new PlejferAlgoritam(tekst, kljuc);
                    sifrovanaPoruka = alg.Enkriptuj();
                    Console.WriteLine("[SIFROVANO]: " + sifrovanaPoruka);
                }
                else if (izborSifrovanja == "3")
                {
                    Console.WriteLine("[IZABRANO SIFROVANJE]: Keyword");
                    algoritam = "Keyword";
                    KeywordAlgoritam alg = new KeywordAlgoritam(tekst, kljuc);
                    sifrovanaPoruka = alg.Enkriptuj();
                    Console.WriteLine("[SIFROVANO]: " + sifrovanaPoruka);
                }
                else
                {
                    Console.WriteLine("Pogresan unos.");
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
                    Console.WriteLine("[STATUS]: Poruka poslata serveru.");

                    byte[] buffer = new byte[2048];
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
                   // Console.WriteLine("\n[SERVER ODGOVOR]:" + sifrovanOdgovor + "\n");


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

                    // Console.WriteLine("[DESIFROVAN ODGOVOR]: " + desifrovaniOdgovor);

                    Console.WriteLine("\n" + new string('-', 60));
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [STATUS] -> Odgovor primljen");
                    Console.WriteLine($"[ALGORITAM   ] -> {algoritam}");
                    Console.WriteLine($"[SIFROVANO   ] -> {sifrovanOdgovor}");
                    Console.WriteLine($"[DESIFROVANO ] -> {desifrovaniOdgovor}");
                    Console.WriteLine();
                    Console.WriteLine(new string('-', 60));
                    

                }
                catch (Exception ex)
                {
                    Console.WriteLine("[Greska u komunikaciji]: " + ex.Message);
                    break;
                }
            }

            klijentSoket.Close();
            Console.WriteLine("[STATUS]: Klijent zavrsava sa radom.\nPritisnite bilo koji taster za kraj rada...\n");
            Console.ReadKey();
        }



    }
}
