using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace SAGM.Data.Entities
{
    public class WorkOrderDetailProcess
    {
        [Key]
        public int WorkOrderDetailProcessId { get; set; }

        [Display(Name = "Partida")]
        public WorkOrderDetail WorkOrderDetail { get; set; }


        [Display(Name = "Maquina")]
        [JsonIgnore]
        public Machine Machine { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cantidad")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Costo x unidad")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Cost { get; set; } = 200;


        [Display(Name = "Unidad")]
        public Unit Unit { get; set; }


    }
}
