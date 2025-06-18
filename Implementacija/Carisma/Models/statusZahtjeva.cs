using System.ComponentModel.DataAnnotations;

namespace Carisma.Models
{
    public enum statusZahtjeva
    {
        [Display(Name = "Otvoren")]
        Otvoren = 0,
        [Display(Name = "U obradi")]
        UObradi = 1,
        [Display(Name = "Čeka odgovor")]
        CekaOdgovor = 2,
        [Display(Name = "Zatvoren")]
        Zatvoren = 3
    }
}