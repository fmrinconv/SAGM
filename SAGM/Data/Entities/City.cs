using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SAGM.Data.Entities
{
    public class City
    {
        public int CityId { get; set; }

        [Display(Name = "Ciuadad")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string CityName { get; set; }

        [JsonIgnore]
        public State State { get; set; }

  


    }
}
