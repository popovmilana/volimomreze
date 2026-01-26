using System.Net;

public class NacinKomunikacije
{
    public IPEndPoint klijentAdresa { get; set; }
    public string algoritam { get; set; }
    public string kljuc { get; set; }
    public string poruka { get; set; }
    public NacinKomunikacije(string algoritam, string kljuc, string poruka, IPEndPoint adresa = null)
    {
        this.algoritam = algoritam;
        this.kljuc = kljuc;
        this.poruka = poruka;
        this.klijentAdresa = adresa;
    }

    public NacinKomunikacije() { }
}