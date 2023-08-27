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

        public ICollection<State> States { get; set; }

        [Display(Name = "Estados")]
        public int StatesNumber => States == null ? 0 : States.Count;

        [Display(Name = "Ciudades")]
        public int CitiesNumber => States == null ? 0 : States.Sum(s => s.Cities.Count);
    }
}
