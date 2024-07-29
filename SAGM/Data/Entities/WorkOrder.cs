using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SAGM.Data.Entities
{
    public class WorkOrder
    {
        [Key]
        public int WorkOrderId { get; set; }

        [DefaultValue(0)]
        [Display(Name = "Fecha cotización")]
        public int? QuoteId{ get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Fecha cotización")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime WorkOrderDate { get; set; }

        [Required]
     
        public string WorkOrderName { get; set; }


        [Display(Name = "Creo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public User CreatedBy { get; set; }

        [Display(Name = "Vendedor")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Seller { get; set; }

        [Display(Name = "Cliente")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Customer Customer { get; set; }

        [Display(Name = "Comprador")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public int BuyerContactId { get; set; }

        [Display(Name = "Usuario")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public int FinalUserId { get; set; } //Es el id de un contacto

        [Display(Name = "Fecha vigencia")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? PromiseDate { get; set; }

        [Display(Name = "Fecha modificación")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? ModifyDate { get; set; }


        [Display(Name = "Modificó")]
        [MaxLength(128, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string ModifiedBy { get; set; }

        [StringLength(128)]
        [Display(Name = "OC Cliente")]
        public string CustomerPO { get; set; }

        [Display(Name = "Estatus")]
        [JsonIgnore]
        public WorkOrderStatus WorkOrderStatus { get; set; }

        [Display(Name = "Moneda")]
        public Currency Currency { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tipo de cambio")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal ExchangeRate { get; set; } = 1;

        [Display(Name = "Comentarios")]
        public string Comments { get; set; }


        [Display(Name = "Activa")]
        [DefaultValue(true)]
        public bool Active { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "IVA")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Tax { get; set; }

        public ICollection<WorkOrderDetail> WorkOrderDetails { get; set; }
        public ICollection<WorkOrderComment> WorkOrderComments { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
