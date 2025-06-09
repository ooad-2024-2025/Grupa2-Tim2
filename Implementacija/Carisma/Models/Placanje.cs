namespace Carisma.Models
{
    public class Placanje
    {
        public int Id { get; set; }
        public double iznos { get; set; }
        public DateTime datumPlacanja { get; set; }
        public StatusPlacanja statusPlacanja { get; set; } 
        public String brojKartice { get; set; }
        public Osoba korisnik { get; set; }
        //public Rezervacija rezervacija { get; set; }
       // public int rezervacijaId { get; set; }


        public StatusPlacanja procesirajPlacanje()
        {
            return statusPlacanja;
        }

        public bool validirajKarticu()
        {
            return false;
        }

        public string generisiPotvrdu()
        {
            return "";
        }
        public bool platiOnline(double iznos)
        {
            // TODO: Dodati logiku za online plaćanje
            return false;
        }

    }
}
