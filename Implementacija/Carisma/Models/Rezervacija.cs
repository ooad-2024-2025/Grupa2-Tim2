using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Carisma.Models
{
    public class Rezervacija
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Datum početka je obavezan.")]
        public DateTime datumPocetka { get; set; }

        [Required(ErrorMessage = "Datum završetka je obavezan.")]
        public DateTime datumZavrsetka { get; set; }

        public DateTime datumRezervacije { get; set; } = DateTime.Now;
        public StatusRezervacije Status { get; set; } = StatusRezervacije.UToku;
        public Osoba? korisnik { get; set; }
        public double cijena { get; set; } = 0.0;
        
        public int? korisnikId { get; set; }  // nullable ako je opcionalno
        [ForeignKey("korisnikId")]
       // public Osoba? korisnik { get; set; }

        public int voziloId { get; set; }
        [ForeignKey("voziloId")]
        public Vozilo vozilo { get; set; }

        public Rezervacija() { }



        public double izracunajCijenu(double cijenaPoDanu)
        {
            // Računa broj dana (uključujući i dan završetka)
            int brojDana = (datumZavrsetka.Date - datumPocetka.Date).Days + 1;
            if (brojDana < 1) brojDana = 1; // minimalno jedan dan

            cijena = brojDana * cijenaPoDanu;
            return cijena;
        }

        public bool potvrdiRezervaciju()
        {
            if (Status == StatusRezervacije.Otkazana)
            {
                // Ne može se potvrditi otkazana rezervacija
                return false;
            }

            Status = StatusRezervacije.Zavrsena;
            return true;
        }

        public bool otkaziRezervaciju()
        {
            if (Status == StatusRezervacije.Otkazana)
            {
                // Već je otkazana
                return false;
            }

            Status = StatusRezervacije.Otkazana;
            return true;
        }

        public static Rezervacija rezervisiVozilo(Vozilo vozilo, DateTime datum, Osoba korisnik)
        {
            return new Rezervacija
            {
                vozilo = vozilo,
                datumRezervacije = datum,
                korisnik = korisnik,
                Status = StatusRezervacije.UToku, // ili neki početni status
                cijena = vozilo.CijenaPoDanu // ili izračunaj ako treba drugačije
            };
        }


    }
}
