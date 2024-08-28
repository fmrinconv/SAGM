using SAGM.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SAGM.Models
{
    public class WorkOrderRemisionViewModel
    {
   
        public int WorkOrderDeliveryId { get; set; }
        public int WorkOrderId { get; set; }

        public string WorkOrderDeliveryName { get; set; }


        public WorkOrder WorkOrder { get; set; }

        [Display(Name = "Fecha remisión")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? RemisionDate { get; set; }

        [Display(Name = "Comentarios")]
        public string Comments { get; set; }


        [Display(Name = "Comprador")]
        public string BuyerName { get; set; }

        [Display(Name = "Vendedor")]
        public string SellerName { get; set; }

        [Display(Name = "Usuario")]
        public string FinalUserName { get; set; }

        [Display(Name = "Estatus OT")]
        public string WorkOrderstatus { get; set; }


        [Display(Name = "detalles")]
        [Required(ErrorMessage = "Es necesario introducir por lo menos una partida a remisionar")]
        public string wodeliverydetails { get; set; }
    }
}
