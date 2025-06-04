using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Carisma.Models
{
    public class Vozilo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Marka { get; set; }

        [Required]
        public string Model { get; set; }

        public int Godina { get; set; }

        public double CijenaPoDanu { get; set; }

        public bool Dostupnost { get; set; }

        [ForeignKey("Osoba")]
        public int OsobaId { get; set; }

        public virtual Osoba Osoba { get; set; }

        // Poslovne metode
        public void OznaciKaoNedostupno()
        {
            Dostupnost = false;
        }

        public void OznaciKaoDostupno()
        {
            Dostupnost = true;
        }

        public void PrikaziDetalje()
        {
            Console.WriteLine($"Vozilo: {Marka} {Model}, Godina: {Godina}, Cijena: {CijenaPoDanu}, Dostupno: {Dostupnost}");
        }
    }
}