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

        //public bool Dostupnost { get; set; }

        [ForeignKey("Osoba")]
        public int OsobaId { get; set; }

        public virtual Osoba Osoba { get; set; }

        public Dostupnost Status { get; set; }



        public bool jeDostupno()
        {
            return true; 
        }
        /*
        public Rezervacija rezervisiVozilo(Vozilo vozilo, DateTime datum)
        {
            return new Rezervacija
            {
                Vozilo = vozilo,
                DatumRezervacije = datum,
                Osoba = this
            };
        }
        */

        public bool azurirajVozilo(int id, Vozilo novoVozilo)
        {
            return false; 
        }

        public void pretraziVozilo (Vozilo vozilo)
        {
            Console.WriteLine("Pretraga vozila pokrenuta.");
        }




    }
}