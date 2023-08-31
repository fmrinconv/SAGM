using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace SAGM.Data.Entities
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Display(Name = "Categoria")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string CategoryName { get; set; }

        public ICollection<MaterialType> MaterialTypes { get; set; }

        [Display(Name = "Tipos de Material")]
        public int MaterialTypesNumber => MaterialTypes == null ? 0 : MaterialTypes.Count;

        [Display(Name = "Materiales")]
        public int MaterialesNumber => MaterialTypes == null ? 0 : MaterialTypes.Sum(t => t.Materials.Count);

        public Boolean Active { get; set; }
    }
}
