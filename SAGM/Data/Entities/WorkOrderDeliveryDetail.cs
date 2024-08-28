using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAGM.Data.Entities
{
    public class WorkOrderDeliveryDetail
    {
        [Key]
        public int WorkOrderDeliverDetailyId { get; set; }

        public WorkOrderDelivery workOrderDelivery { get; set; }

        public WorkOrderDetail workOrderDetail { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cantidad")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Quantity { get; set; }
    }
}
