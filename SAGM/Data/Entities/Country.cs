using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SAGM.Data.Entities
{
    public class Country
    {
        public int CountryId { get; set; }

        [Display(Name = "País")]
        [MaxLength(50, ErrorMessage ="El campo {0} debe tener máximo {1} caracteres")]
        [Required(ErrorMessage ="El campo {0} es requerido")]
        public string CountryName { get; set; }
    }
}
