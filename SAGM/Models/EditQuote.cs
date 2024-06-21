using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SAGM.Models
{
    public class EditQuote 
    {

        public int QuoteId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Fecha cotización")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime QuoteDate { get; set; }

        [Display(Name = "Cotización")]
        public string QuoteName { get; set; }

        [Display(Name = "Creo")]
        public string CreatedBy { get; set; }

        [Display(Name = "Vendedor")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string SellerId { get; set; }

        public IEnumerable<SelectListItem> Sellers { get; set; }

        [Display(Name = "Cliente")]
        [Range(1, int.MaxValue, ErrorMessage = "El campo {0} es obligatorio.")]
        public int CustomerId { get; set; }

        public IEnumerable<SelectListItem> Customers { get; set; }

        [Display(Name = "Comprador")]
        [Range(1, int.MaxValue, ErrorMessage = "El campo {0} es obligatorio.")]
        public int BuyerContactId { get; set; } //Id de un contacto


        public IEnumerable<SelectListItem> CustomerBuyerContacts { get; set; }

        [Display(Name = "Usuario")]
        [Range(1, int.MaxValue, ErrorMessage = "El campo {0} es obligatorio.")]
        public int FinalUserId { get; set; } //Es el id de un contacto

        public IEnumerable<SelectListItem> CustomerFinalContacts { get; set; }

        [Display(Name = "Fecha vigencia")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? validUntilDate { get; set; }

        [Display(Name = "Fecha modificación")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? ModifyDate { get; set; }

        [Display(Name = "Modificó")]
        [MaxLength(128, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string? ModifiedById { get; set; }

        public IEnumerable<SelectListItem> ModifiedBy { get; set; }

        [StringLength(128)]
        [Display(Name = "OC Cliente")]
        public string? CustomerPO { get; set; }

        [Display(Name = "Estatus")]
        public IEnumerable<SelectListItem> QuoteStatus { get; set; }

        [Display(Name = "Estatus")]
        public int QuoteStatusId { get; set; }

        [Display(Name = "Moneda")]
        public IEnumerable<SelectListItem> Currency { get; set; }

        [Display(Name = "Moneda")]
        public int CurrencyId { get; set; }

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
    }
}
