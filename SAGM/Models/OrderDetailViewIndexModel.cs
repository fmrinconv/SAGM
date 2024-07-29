using SAGM.Data.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class OrderDetailViewIndexModel
    {

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cantidad")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Quantity { get; set; }

        [Display(Name = "Material")]
        public string Material { get; set; }

        [Display(Name = "Descripción")]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Price { get; set; }

  

        
    }
}
