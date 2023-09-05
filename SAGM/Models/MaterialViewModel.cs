using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Xml.Linq;

namespace SAGM.Models
{
    public class MaterialViewModel
    {
        public int MaterialId { get; set; }

        [Display(Name = "Material")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string MaterialName { get; set; }

        [Display(Name = "Activo")]
        [DefaultValue(true)]
        public Boolean Active { get; set; }

        public int MaterialTypeId { get; set; }
    }
}
