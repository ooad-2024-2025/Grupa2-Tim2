using System.ComponentModel.DataAnnotations;

namespace Carisma.Models
{
    public class Podrska
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Pitanje je obavezno")]
        [StringLength(2000, MinimumLength = 20, ErrorMessage = "Pitanje mora imati između 20 i 2000 karaktera")]
        [Display(Name = "Opis problema")]
        public string Pitanje { get; set; }

        public DateTime? DatumPostavljanja { get; set; }

        [Display(Name = "Odgovor podrške")]
        public string? Odgovor { get; set; }

        [Display(Name = "Status zahtjeva")]
        public statusZahtjeva? Status { get; set; }

        [Display(Name = "Ima novi odgovor")]
        public bool ImaNoviOdgovor { get; set; } = false;

        [Required]
        [Display(Name = "Nivo hitnosti")]
        public NivoHitnosti Hitnost { get; set; } = NivoHitnosti.Srednja;

        public string? KorisnikId { get; set; }  // NULL za neregistrovane korisnike

        public string? PodrskaKorisnikId { get; set; }  // Ko je odgovorio

        public DateOnly? DatumOdgovora { get; set; }

        [Display(Name = "Ocjena korisnika")]
        [Range(1, 5, ErrorMessage = "Ocjena mora biti između 1 i 5")]
        public int? OcjenaKorisnika { get; set; }

        [Display(Name = "Komentar o usluzi")]
        [StringLength(500, ErrorMessage = "Komentar ne može biti duži od 500 karaktera")]
        public string? KomentarOcjene { get; set; }

        // Dodano za neregistrovane korisnike
        [Display(Name = "Email adresa")]
        [EmailAddress(ErrorMessage = "Unesite validnu email adresu")]
        public string? Email { get; set; }

        [Display(Name = "Ime i prezime")]
        [StringLength(100, ErrorMessage = "Ime i prezime ne mogu biti duži od 100 karaktera")]
        public string? ImePrezime { get; set; }

        // Helper properties
        public string PrioritetniBadge => Hitnost switch
        {
            NivoHitnosti.Niska => "badge bg-success",
            NivoHitnosti.Srednja => "badge bg-primary",
            NivoHitnosti.Visoka => "badge bg-warning",
            NivoHitnosti.Kriticna => "badge bg-danger",
            _ => "badge bg-secondary"
        };

        public string StatusBadge => Status switch
        {
            statusZahtjeva.Otvoren => "badge bg-info",
            statusZahtjeva.UObradi => "badge bg-warning",
            statusZahtjeva.CekaOdgovor => "badge bg-secondary",
            statusZahtjeva.Zatvoren => "badge bg-success",
            _ => "badge bg-secondary"
        };

        // Helper property za prikaz korisnika
        public bool IsRegisteredUser => !string.IsNullOrEmpty(KorisnikId);
    }
}