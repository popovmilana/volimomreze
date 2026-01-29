using System;

namespace CommonLib
{
    public class PlejferAlgoritam
    {
        public string Poruka { get; set; }
        public string Kljuc { get; set; }
        private char[,] matrica = new char[5, 5];

        public PlejferAlgoritam(string poruka, string kljuc)
        {
            this.Poruka = poruka;
            this.Kljuc = kljuc;
            PopuniMatricu(kljuc);
        }

        private void PopuniMatricu(string kljuc)
        {
            char[] pomocniNiz = new char[25];
            int brojac = 0;
            string alfabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
            string sve = kljuc.ToUpper().Replace("J", "I") + alfabet;

            for (int i = 0; i < sve.Length; i++)
            {
                char trenutno = sve[i];
                if (trenutno < 'A' || trenutno > 'Z') continue;

                bool postoji = false;
                for (int j = 0; j < brojac; j++)
                {
                    if (pomocniNiz[j] == trenutno) { postoji = true; break; }
                }

                if (!postoji && brojac < 25)
                {
                    pomocniNiz[brojac++] = trenutno;
                }
            }

            int k = 0;
            for (int r = 0; r < 5; r++)
                for (int c = 0; c < 5; c++)
                    matrica[r, c] = pomocniNiz[k++];
        }

        private string PripremiTekst(string tekst)
        {
            string s = tekst.ToUpper().Replace("J", "I").Replace(" ", "");
            char[] privremeni = new char[s.Length * 2];
            int n = 0;

            for (int i = 0; i < s.Length; i++)
            {
                privremeni[n++] = s[i];
                if (i + 1 < s.Length && s[i] == s[i + 1] && n % 2 != 0)
                {
                    privremeni[n++] = 'X';
                }
            }

            if (n % 2 != 0) privremeni[n++] = 'X';

            return new string(privremeni, 0, n);
        }

        public string Enkriptuj()
        {
            string pripremljeno = PripremiTekst(Poruka);
            return Transformisi(pripremljeno, 1);
        }

        public string Dekriptuj()
        {
            string s = Poruka.ToUpper().Replace(" ", "").Replace("J", "I");
            string desifrovanoRaw = Transformisi(s, 4);

            char[] rezultatNiz = new char[desifrovanoRaw.Length];
            int n = 0;

            for (int i = 0; i < desifrovanoRaw.Length; i++)
            {
                // brisemo X samo ako je on bio dopunski karakter.
                // U Playfair-u, dopunski X se uvek nalazi na PARNOJ poziciji indeksi 1, 3, 5...

                bool jeNaParnomMestu = (i % 2 != 0);

                if (jeNaParnomMestu && desifrovanoRaw[i] == 'X' && i + 1 < desifrovanoRaw.Length)
                {
                    if (desifrovanoRaw[i - 1] == desifrovanoRaw[i + 1])
                    {
                        continue;
                    }
                }

                if (i == desifrovanoRaw.Length - 1 && desifrovanoRaw[i] == 'X')
                {
                    continue;
                }

                rezultatNiz[n++] = desifrovanoRaw[i];
            }

            return new string(rezultatNiz, 0, n);
        }

        private void GdeJeSlovo(char c, out int red, out int kol)
        {
            red = 0; kol = 0;
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                    if (matrica[i, j] == c) { red = i; kol = j; return; }
        }

        private string Transformisi(string tekst, int pomeraj)
        {
            char[] rezultat = new char[tekst.Length];
            for (int i = 0; i < tekst.Length; i += 2)
            {
                GdeJeSlovo(tekst[i], out int r1, out int k1);
                GdeJeSlovo(tekst[i + 1], out int r2, out int k2);

                if (r1 == r2)
                {
                    rezultat[i] = matrica[r1, (k1 + pomeraj) % 5];
                    rezultat[i + 1] = matrica[r2, (k2 + pomeraj) % 5];
                }
                else if (k1 == k2)
                {
                    rezultat[i] = matrica[(r1 + pomeraj) % 5, k1];
                    rezultat[i + 1] = matrica[(r2 + pomeraj) % 5, k2];
                }
                else
                {
                    rezultat[i] = matrica[r1, k2];
                    rezultat[i + 1] = matrica[r2, k1];
                }
            }
            return new string(rezultat);
        }
    }
}