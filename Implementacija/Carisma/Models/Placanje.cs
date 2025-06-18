using System.ComponentModel.DataAnnotations.Schema;

namespace Carisma.Models
{
    public class Placanje
    {
        public int Id { get; set; }
        public double iznos { get; set; }
        public DateTime datumPlacanja { get; set; }
        public StatusPlacanja statusPlacanja { get; set; } 
        public String? brojKartice { get; set; }
        
        public int korisnikId { get; set; }
        [ForeignKey("korisnikId")]
        public virtual Osoba korisnik { get; set; }
        
        public int? RezervacijaId { get; set; }
        [ForeignKey("RezervacijaId")]
        public virtual Rezervacija Rezervacija { get; set; }
        
        // Stripe-specifična polja
        public string? StripeSessionId { get; set; }
        public string? StripePaymentIntentId { get; set; }
        
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