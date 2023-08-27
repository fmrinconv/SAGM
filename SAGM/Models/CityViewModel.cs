using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class CityViewModel
    {
        public int CityId { get; set; }
        [Display(Name = "Ciudad")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string CityName { get; set; }

        public int StateId { get; set; }
    }
}
