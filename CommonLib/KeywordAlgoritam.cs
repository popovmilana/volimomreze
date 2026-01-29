using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib
{
    public class KeywordAlgoritam
    {
        public string Poruka { get; set; }
        public string Kljuc { get; set; }
        private string sifrovanaAbeceda;
        private const string NORMALNA_ABECEDA = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public KeywordAlgoritam(string poruka, string kljuc)
        {
            this.Poruka = poruka;
            this.Kljuc = kljuc;
            this.sifrovanaAbeceda = KreirajSifrovanAbecdu(kljuc);
        }

        private string KreirajSifrovanAbecdu(string kljuc)
        {
            StringBuilder abeceda = new StringBuilder();
            HashSet<char> iskoriscena = new HashSet<char>();

            // Dodaj jedinstvene karaktere iz ključa
            foreach (char c in kljuc.ToUpper())
            {
                if (char.IsLetter(c) && !iskoriscena.Contains(c))
                {
                    abeceda.Append(c);
                    iskoriscena.Add(c);
                }
            }

            // Dodaj preostale karaktere iz alfabeta
            foreach (char c in NORMALNA_ABECEDA)
            {
                if (!iskoriscena.Contains(c))
                {
                    abeceda.Append(c);
                }
            }

            return abeceda.ToString();
        }

        public string Enkriptuj()
        {
            StringBuilder rezultat = new StringBuilder();

            foreach (char c in Poruka.ToUpper())
            {
                if (c >= 'A' && c <= 'Z')
                {
                    int index = c - 'A';
                    rezultat.Append(sifrovanaAbeceda[index]);
                }
                else
                {
                    rezultat.Append(c); // Zadrži ne-alfabetske karaktere
                }
            }

            return rezultat.ToString();
        }

        public string Dekriptuj()
        {
            StringBuilder rezultat = new StringBuilder();

            foreach (char c in Poruka.ToUpper())
            {
                if (c >= 'A' && c <= 'Z')
                {
                    int index = sifrovanaAbeceda.IndexOf(c);
                    if (index != -1)
                    {
                        rezultat.Append(NORMALNA_ABECEDA[index]);
                    }
                    else
                    {
                        rezultat.Append(c);
                    }
                }
                else
                {
                    rezultat.Append(c); // Zadrži ne-alfabetske karaktere
                }
            }

            return rezultat.ToString();
        }
    }
}
