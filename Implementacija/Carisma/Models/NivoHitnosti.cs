using System.ComponentModel.DataAnnotations;

namespace Carisma.Models
{
    public enum NivoHitnosti
    {
        [Display(Name = "Niska")]
        Niska = 0,
        [Display(Name = "Srednja")]
        Srednja = 1,
        [Display(Name = "Visoka")]
        Visoka = 2,
        [Display(Name = "Kritična")]
        Kriticna = 3
    }
}