using Carisma.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Carisma.Models
{
    public class Osoba
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        public string Kontakt { get; set; }
        public string Telefon { get; set; }
        public string Kontofile { get; set; }

        public Uloga Uloga { get; set; }

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
}
