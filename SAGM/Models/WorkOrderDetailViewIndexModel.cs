using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class WorkOrderDetailViewIndexModel
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
        [Display(Name = "Materia prima")]
        public decimal RawMaterial { get; set; } = 0;

        [Display(Name = "Maquinado")]
        public decimal Machined { get; set; } = 0;

        [Display(Name = "Tratamiento")]
        public decimal TT { get; set; } = 0;

        [Display(Name = "Embarcado")]
        public decimal Shipped { get; set; } = 0;

        [Display(Name = "Facturado")]
        public decimal Invoiced { get; set; } = 0;

        public bool Completed { get; set; } = false;
    }
}
