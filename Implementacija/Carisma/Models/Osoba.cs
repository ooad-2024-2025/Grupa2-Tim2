using System.ComponentModel.DataAnnotations;

namespace Carisma.Models
{
    public class Osoba
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Unesite ispravan format email adrese.")]
        public string email { get; set; }

        public string lozinka { get; set; }

        [Phone(ErrorMessage = "Unesite ispravan format telefonskog broja.")]
        [StringLength(20, ErrorMessage = "Broj telefona ne smije biti duži od 20 karaktera.")]
        public string broj_telefona { get; set; }

        [Required(ErrorMessage = "Korisničko ime je obavezno.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Korisničko ime mora imati između 3 i 50 karaktera.")]
        public string korisnicko_ime { get; set; }

        // Dodaj ova nova svojstva za integraciju sa Identity
        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(50, ErrorMessage = "Ime ne može biti duže od 50 karaktera.")]
        public string Ime { get; set; }

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(50, ErrorMessage = "Prezime ne može biti duže od 50 karaktera.")]
        public string Prezime { get; set; }

        public string? IdentityUserId { get; set; }  // Povezivanje sa Identity korisnicima

        public Uloga? uloga { get; set; }
        public bool blokiran { get; set; } = false;

        // Navigation property za zahtjeve podrške
        public ICollection<Podrska> ZahtjeviPodrske { get; set; } = new List<Podrska>();

        public Osoba() { }

        public string PunoIme => $"{Ime} {Prezime}";

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