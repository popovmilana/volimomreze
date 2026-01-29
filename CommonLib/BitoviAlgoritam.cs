using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib
{
    public class BitoviAlgoritam
    {
        public string Poruka { get; set; }
        public string Kljuc { get; set; }

        public BitoviAlgoritam(string poruka, string kljuc)
        {
            this.Poruka = poruka;
            this.Kljuc = kljuc;
        }

        public string Enkriptuj()
        {
            if (string.IsNullOrEmpty(Kljuc))
                return Poruka;

            StringBuilder rezultat = new StringBuilder();

            for (int i = 0; i < Poruka.Length; i++)
            {
                // Pretvoramo karakter u binarni string (8 bita)
                string binarniKarakter = Convert.ToString(Poruka[i], 2).PadLeft(8, '0');

                // Pretvoramo odgovarajući karakter ključa u binarni string
                char kljucKarakter = Kljuc[i % Kljuc.Length];
                string binarniKljuc = Convert.ToString(kljucKarakter, 2).PadLeft(8, '0');

                // XOR operacija bit po bit
                string xorRezultat = "";
                for (int j = 0; j < 8; j++)
                {
                    xorRezultat += (binarniKarakter[j] == binarniKljuc[j]) ? '0' : '1';
                }

                // Konvertujemo binarni string nazad u karakter
                int vrednost = Convert.ToInt32(xorRezultat, 2);
                rezultat.Append((char)vrednost);
            }

            // Konvertujemo StringBuilder direktno u byte array 
            byte[] bytes = new byte[rezultat.Length];
            for (int i = 0; i < rezultat.Length; i++)
            {
                bytes[i] = (byte)rezultat[i];
            }

            // Vratimo kao Base64 string za lakši prenos
            return Convert.ToBase64String(bytes);
        }

        public string Dekriptuj()
        {
            try
            {
                // Dekodirajemo iz Base64
                byte[] podaci = Convert.FromBase64String(Poruka);

                if (string.IsNullOrEmpty(Kljuc))
                    return Encoding.UTF8.GetString(podaci);

                StringBuilder rezultat = new StringBuilder();

                for (int i = 0; i < podaci.Length; i++)
                {
                    // Pretvorimo byte u binarni string (8 bita)
                    string binarniKarakter = Convert.ToString(podaci[i], 2).PadLeft(8, '0');

                    // Pretvorimo odgovarajući karakter ključa u binarni string
                    char kljucKarakter = Kljuc[i % Kljuc.Length];
                    string binarniKljuc = Convert.ToString(kljucKarakter, 2).PadLeft(8, '0');

                    // XOR operacija bit po bit (ista operacija za dešifrovanje)
                    string xorRezultat = "";
                    for (int j = 0; j < 8; j++)
                    {
                        xorRezultat += (binarniKarakter[j] == binarniKljuc[j]) ? '0' : '1';
                    }

                    // Konvertujemo binarni string nazad u karakter
                    int vrednost = Convert.ToInt32(xorRezultat, 2);
                    rezultat.Append((char)vrednost);
                }

                return rezultat.ToString();
            }
            catch
            {
                return "Greška pri dešifrovanju!";
            }
        }
    }

}
