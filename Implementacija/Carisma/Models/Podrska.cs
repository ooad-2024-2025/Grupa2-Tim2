namespace Carisma.Models
{
    public class Podrska
    {
        public int Id { get; set; }
        public String Pitanje { get; set; }
        public DateTime? DatumPostavljanja { get; set; }
        public String? Odgovor { get; set; }
        public statusZahtjeva? Status { get; set; }
        public Osoba Korisnik { get; set; }
        public DateOnly? DatumOdgovora { get; set; }


        public static void podrskaKorisniku(Osoba korisnik, String poruka) {
            Console.WriteLine("Korisnki {korisnik} je poslao poruku: {poruka}");
        }

        public static bool posaljiNotifikaciju()
        {
            return true;
        }

        public static void evidentirajInterakciju()
        {

        }

    }

   
}
