using System.ComponentModel.DataAnnotations;

namespace SAGM.Data.Entities
{
    public class OrderStatus
    {

        [Key]
        public int OrderStatusId { get; set; }

        [Display(Name = "Estatus")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string OrderStatusName { get; set; }
        public ICollection<Order> Order { get; set; }
    }
}
