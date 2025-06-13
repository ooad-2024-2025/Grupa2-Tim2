using System.ComponentModel.DataAnnotations;

namespace Carisma.Models
{
    public enum NivoHitnosti
    {
        [Display(Name = "Niska")]
        Niska = 1,
        [Display(Name = "Srednja")]
        Srednja = 2,
        [Display(Name = "Visoka")]
        Visoka = 3,
        [Display(Name = "Kritična")]
        Kriticna = 4
    }
}
