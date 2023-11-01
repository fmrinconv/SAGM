using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAGM.Data.Entities
{
    public class Quote
    {
        [Key]
        public int QuoteId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Fecha cotización")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime QuoteDate { get; set; }

        [Required]
        public string QuoteName { get; set; }

        
        [Required]
        [Display(Name = "Creo")]
        public User CreatedBy { get; set; }

        [Display(Name = "Vendedor")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public User Seller { get; set; }

        [Display(Name = "Cliente")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Customer Customer { get; set; }

        [Display(Name = "Comprador")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Contact Buyer { get; set; }

        [Display(Name = "Usuario")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public int FinalUser { get; set; }

        [Display(Name = "Fecha vigencia")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? validUntilDate { get; set; }

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
        QuoteStatus QuoteStatus { get; set; }

        [Display(Name = "Moneda")]
        public Currency Currency { get; set; }

        [Display(Name = "Comentarios")]
        public string? Comments { get; set; }


        [Display(Name = "Activa")]
        [DefaultValue(true)]
        public Boolean Active { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "IVA")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Tax { get; set; }

        public ICollection<QuoteDetail> QuoteDetails { get; set; }

    }
}
