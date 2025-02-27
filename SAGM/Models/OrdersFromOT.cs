using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using SAGM.Data;
namespace SAGM.Models
{
    public class OrdersFromOT
    {

        public int OrderId { get; set; }


        [Display(Name = "Fecha cotización")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Orden de compra")]
        public string OrderName { get; set; }

        [Display(Name = "Comprador")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string CreatedBy { get; set; }

        [Display(Name = "Comprador")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Buyer { get; set; }

        [Display(Name = "Proveedor")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Supplier { get; set; }

        [Display(Name = "Vendedor")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string SupplierContact { get; set; }

        [Display(Name = "Fecha compromiso")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? EstimatedDeliveryDate { get; set; }

        [Display(Name = "Fecha entrega")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? DeliveryDate { get; set; } = null;

        [StringLength(128)]
        [Display(Name = "Cotización")]
        public string? SupplierQuote { get; set; }

        [Display(Name = "Estatus")]
        [JsonIgnore]
        public string OrderStatus { get; set; }

        [Display(Name = "Moneda")]
        public string Currency { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tipo de cambio")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal ExchangeRate { get; set; } = 1;

        [Display(Name = "Comentarios")]
        public string? Comments { get; set; }


        [Display(Name = "Activa")]
        [DefaultValue(true)]
        public Boolean Active { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "IVA")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Tax { get; set; }

        public ICollection<Data.Entities.OrderDetail> OrderDetails { get; set; }
        public ICollection<Data.Entities.OrderComment> OrderComments { get; set; }

        public Data.Entities.WorkOrder WorkOrder { get; set; }

        public decimal Subtotal { get; set; }
    }
}
