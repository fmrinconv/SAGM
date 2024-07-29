using SAGM.Data.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAGM.Models
{
    public class OrderViewIndexModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Fecha cotización")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime OrderDate { get; set; }

        [Required]
        public string OrderName { get; set; }


        [Display(Name = "Creo")]
        public string CreatedBy { get; set; }

        [Display(Name = "Vendedor")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Buyer { get; set; }

        [Display(Name = "Cliente")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public String SupplierNickName { get; set; }

        [Display(Name = "Comprador")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string SellerContact { get; set; }


        [Display(Name = "Fecha modificación")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? EstimatedDeliveryDate { get; set; }

        [Display(Name = "Fecha modificación")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? DeliveryDate { get; set; } = null;

        [Display(Name = "Modificó")]
        [MaxLength(128, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string? ModifiedBy { get; set; }

        [StringLength(128)]
        [Display(Name = "Cotización")]
        public string? SupplierQuote { get; set; }

        [Display(Name = "Estatus")]

        public string OrderStatusName { get; set; }

        [Display(Name = "Moneda")]
        public string Currency { get; set; }

        [Display(Name = "Comentarios")]
        public string? Comments { get; set; }

        [Display(Name = "Activa")]
        [DefaultValue(true)]
        public Boolean Active { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "IVA")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Tax { get; set; }

        public ICollection<OrderDetailViewIndexModel> OrderDetails { get; set; }

        public int ArchivesNumber { get; set; }

        public string ArchivesChain { get; set; }

        public decimal total => OrderDetails.Count == 0 ? 0 : OrderDetails.Sum(q => q.Price * q.Quantity);

        public int WorkOrderid { get; set; }
        public string WorkOrderName { get; set; }


    }
}
