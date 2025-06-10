using System.ComponentModel.DataAnnotations;

namespace Carisma.Models
{
    public enum StatusRezervacije
    {
        [Display(Name = "Kreirana")]
        Kreirana,
        [Display(Name = "U toku")]
        UToku,
        [Display(Name = "Završena")]
        Zavrsena,
        [Display(Name = "Otkazana")]
        Otkazana
    }
}