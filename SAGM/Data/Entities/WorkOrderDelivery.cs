using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SAGM.Data.Entities
{
    public class WorkOrderDelivery
    {
        [Key]
        public int WorkOrderDeliveryId { get; set; }

        [Required]
        [Display(Name = "Remisión")]
        public string WorkOrderDeliveryName { get; set; }

        [JsonIgnore]
        public WorkOrder WorkOrder { get; set; }

        [Display(Name = "Fecha remisión")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? RemisionDate { get; set; }

        [Display(Name = "Comentarios")]
        public string Comments { get; set; }

        [Display(Name = "Creo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")] 
        public string? Creator { get; set; } = "frincon@simaqap.com";

        public ICollection<WorkOrderDeliveryDetail> WorkOrderDeliveryDetails { get; set; }


    }
}
