using Carisma.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Carisma.Models
{
   
    public class Osoba
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Unesite ispravan format email adrese.")]
        
        public String email { get; set; }
        public String lozinka { get; set; }

        [Phone(ErrorMessage = "Unesite ispravan format telefonskog broja.")]
        [StringLength(20, ErrorMessage = "Broj telefona ne smije biti duži od 20 karaktera.")]
        public String broj_telefona { get; set; }

        [Required(ErrorMessage = "Korisničko ime je obavezno.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Korisničko ime mora imati između 3 i 50 karaktera.")]
        public String korisnicko_ime { get; set; }
        public Uloga? uloga { get; set; }
        public bool blokiran { get; set; } = false;

        public Osoba() { }
        public void registracija()
        {
            Console.WriteLine("Registracija korisnika započeta." + korisnicko_ime);
        }

        public bool blokirajKorisnika(int id)
        {
            if (this.Id == id)
            {
                blokiran = true;
                Console.WriteLine("Korisnik je blokiran.");
                return true;
            }
            else
            {
                Console.WriteLine("Nepostojeći korisnik.");
                return false;

            }
        }

        public void pregledajKorisnika()
        {
            Console.WriteLine("Pregled korisnika je pozvan.");
        }

        public bool odgovoriNaZahtjev(int idZahtjeva, string odgovor)
        {
            Console.WriteLine($"Odgovor na zahtjev: {idZahtjeva}: {odgovor}");
            return true;
        }

        private int brojUspjesnihPlacanja;
        public void SetbrojUspjesnihPlacanja(int broj)
        {
            Console.WriteLine($"Broj uspjesnih placanja je: {brojUspjesnihPlacanja}");
        }

        

    }
}
