using System.ComponentModel.DataAnnotations;

namespace Carisma.Models
{
    public enum StatusPlacanja
    {
        [Display(Name = "Uspješno")]
        Uspjesno,
        [Display(Name = "Neuspješno")]
        Neuspjesno,
        [Display(Name = "Na čekanju")]
        NaCekanju
    }
}
