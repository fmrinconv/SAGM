using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SAGM.Data.Entities
{
    public class WorkOrderDetail
    {
        [Key]
        public int WorkOrderDetailId { get; set; }

        [Display(Name = "Orden de trabajo")]
        public WorkOrder WorkOrder { get; set; }

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cantidad")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Price { get; set; }

        public Material Material { get; set; }

        public Unit Unit { get; set; }

        [Display(Name = "Materia prima")]
        public decimal RawMaterial { get; set; } = 0;

        [Display(Name = "Maquinado")]
        public decimal Machined { get; set; } = 0;

        [Display(Name = "Tratamiento")]
        public decimal TT { get; set; } = 0;

        [Display(Name = "Embarcado")]
        public decimal Shipped { get; set; }= 0;

        [Display(Name = "Facturado")]
        public decimal Invoiced { get; set; } = 0;

        public bool Completed { get; set; } = false;

        public ICollection<WorkOrderDetailComment> WorkOrderDetailComments { get; set; }

        public ICollection<WorkOrderDetailProcess> WorkOrderDetailProcess { get; set; }

        public ICollection<WorkOrderDeliveryDetail> WorkOrderDeliveryDetails { get; set; }
    }
}
