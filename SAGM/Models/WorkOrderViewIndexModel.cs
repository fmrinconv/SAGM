using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SAGM.Models
{
    public class WorkOrderViewIndexModel
    {
        public int WorkOrderId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Fecha cotización")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime WorkOrderDate { get; set; }

        [Required]
        public string WorkOrderName { get; set; }


        [Display(Name = "Creo")]
        public string CreatedBy { get; set; }

        [Display(Name = "Vendedor")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Seller { get; set; }

        [Display(Name = "Cliente")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public String CustomerNickName { get; set; }

        [Display(Name = "Comprador")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string BuyerContact { get; set; }

        [Display(Name = "Usuario")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string FinalUser { get; set; } //Es el id de un contacto

        [Display(Name = "Fecha vigencia")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? PromiseDate { get; set; }

        [Display(Name = "Fecha modificación")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? ModifyDate { get; set; }

        [Display(Name = "Modificó")]
        [MaxLength(128, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string? ModifiedBy { get; set; }

        [StringLength(128)]
        [Display(Name = "OC Cliente")]
        public string? CustomerPO { get; set; }

        [Display(Name = "Estatus")]

        public string WorkOrderStatusName { get; set; }

        [Display(Name = "Moneda")]
        public string Currency { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tipo de cambio")]
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

        public int QuoteId { get; set; }

        public string QuoteName { get; set; }
        public ICollection<WorkOrderDetailViewIndexModel> WorkOrderDetails { get; set; }

        public int ArchivesNumber { get; set; }

        public string ArchivesChain { get; set; }

        public decimal total => WorkOrderDetails.Count == 0 ? 0 : WorkOrderDetails.Sum(q => q.Price * q.Quantity);

    }
}
