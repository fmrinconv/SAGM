using SAGM.Data.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAGM.Models
{
    public class QuoteViewIndexModel
    {
        public int QuoteId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Fecha cotización")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime QuoteDate { get; set; }

        [Required]
        public string QuoteName { get; set; }


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
        public DateTime? validUntilDate { get; set; }

        [Display(Name = "Fecha modificación")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? ModifyDate { get; set; }

        [Display(Name = "Modificó")]
        [MaxLength(128, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string? ModifiedBy { get; set; }

        [StringLength(128)]
        [Display(Name = "Req Cliente")]
        public string? CustomerPO { get; set; }

        [Display(Name = "Estatus")]

        public string QuoteStatusName { get; set; }

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

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Descuento")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Discount { get; set; } = 0;

        public ICollection<QuoteDetailViewIndexModel> QuoteDetails { get; set; }

        public int ArchivesNumber { get; set; }

        public string ArchivesChain { get; set; }

        public decimal total => QuoteDetails.Count == 0 ? 0 : QuoteDetails.Sum(q => q.Price * q.Quantity);


    }
}
