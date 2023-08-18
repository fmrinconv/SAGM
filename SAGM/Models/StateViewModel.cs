using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class StateViewModel
    {
        public int StateId { get; set; }
        [Display(Name = "Estado")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string StateName { get; set; }

        public int CountryId { get; set; }
    }
}
