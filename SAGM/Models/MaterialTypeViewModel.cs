using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Xml.Linq;

namespace SAGM.Models
{
    public class MaterialTypeViewModel
    {
        public int MaterialTypeId { get; set; }
        [Display(Name = "Tipo de material")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string MaterialTypeName { get; set; }

        [Display(Name = "Activo")]
        [DefaultValue(true)]
        public Boolean Active { get; set; }

        public int CategoryId { get; set; }
    }
}
