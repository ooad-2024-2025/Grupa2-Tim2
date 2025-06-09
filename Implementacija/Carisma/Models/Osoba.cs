using Carisma.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Carisma.Models
{
    /*
    public class Osoba
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        public string? Kontakt { get; set; }
        public string Telefon { get; set; }
        public string? Kontofile { get; set; }

        public Uloga Uloga { get; set; } = Uloga.RegistrovaniKorisnik;
        public string? IdKorisnika { get; set; }




        public virtual ICollection<Vozilo> Vozila { get; set; } = new List<Vozilo>();

        // Poslovna logika
        public void KontaktirajPodrsku(string poruka)
        {
            Console.WriteLine($"Poruka podršci: {poruka}");
        }

        public bool Registruj()
        {
            this.Uloga = Uloga.RegistrovaniKorisnik;
            return true;
        }

        public void PregledajKorisnika()
        {
            Console.WriteLine($"Korisnik: {Email}, Uloga: {Uloga}");
        }

        public bool AzurirajVozilo(int voziloId, Vozilo novoVozilo)
        {
            var vozilo = Vozila.FirstOrDefault(v => v.Id == voziloId);
            if (vozilo != null)
            {
                vozilo.Marka = novoVozilo.Marka;
                vozilo.Model = novoVozilo.Model;
                return true;
            }
            return false;
        }
    }
    */


    public class Osoba
    {
        [Key]
        public int Id { get; set; }
        public String email { get; set; }
        public String lozinka { get; set; }
        public String broj_telefona { get; set; }
        public String korisnicko_ime { get; set; }
        public Uloga? uloga { get; set; }
        public bool blokiran { get; set; } = false;
        //public bool prihvacena { get; set; } = false;

        /*
        public void kontaktirajPodrsku()
        {
            Console.WriteLine("Kontaktirali ste korisnčku podršku.");
        }
        */
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

        /*public void pretraziVozila()
        {
            Console.WriteLine("Pretrazujem vozilo...");
        }*/

        /*
        public bool platiOnline(double iznos, bool doznaka)
        {
            if (doznaka == true)
            {
                Console.WriteLine("Online placanje izvrseno.");
                return true;
            }
            else
            {
                Console.WriteLine("Kartica odbijena");
                return false;
            }
        }
        */

        /*public bool azurirajVozilo(int id, Vozilo novoVozilo)
        {
            Console.WriteLine("Azuriranje vozila");
            return true;
        }*/

    }
}
