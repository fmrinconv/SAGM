using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using System.Text.Json.Serialization;
namespace SAGM.Data.Entities
{
    public class Material
    {

        [Key]
        public int MaterialId { get; set; }

        [Display(Name = "Material")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string MaterialName { get; set; }

        public Boolean Active { get; set; }

        [JsonIgnore]
        public MaterialType MaterialType { get; set; }

        public ICollection<QuoteDetail> QuoteDetails { get; set; }


    }
}
