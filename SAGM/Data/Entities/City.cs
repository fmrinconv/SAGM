using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SAGM.Data.Entities
{
    public class City
    {
        [Key]
        public int CityId { get; set; }

        [Display(Name = "Ciudad")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string CityName { get; set; }

        [Display(Name = "Activo")]
        [DefaultValue(true)]
        public Boolean Active { get; set; }

        [JsonIgnore]
        public State State { get; set; }

        public ICollection<User> Users{ get; set; }

        public ICollection<Customer> Customer{ get; set; }




    }
}
