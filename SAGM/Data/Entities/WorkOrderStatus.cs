using System.ComponentModel.DataAnnotations;

namespace SAGM.Data.Entities
{
    public class WorkOrderStatus
    {
        [Key]
        public int WorkOrderStatusId { get; set; }

        [Display(Name = "Estatus")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string WorkOrderStatusName { get; set; }
        public ICollection<WorkOrder> WorkOrder { get; set; }
    }

}
