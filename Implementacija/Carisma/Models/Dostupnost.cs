using System.ComponentModel.DataAnnotations;

namespace Carisma.Models
{
    public enum Dostupnost
    {
        [Display(Name = "Dostupno")]
        Dostupno,

        [Display(Name = "Nije dostupno")]
        NijeDostupno
    }
}
