using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SAGM.Models
{
    public class EditOrder
    {

        public int OrderId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Fecha cotización")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Cotización")]
        public string OrderName { get; set; }

        [Display(Name = "Creo")]
        public string CreatedBy { get; set; }

        [Display(Name = "Comprador")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string BuyerId { get; set; }

        public IEnumerable<SelectListItem> Buyers { get; set; }

        [Display(Name = "Proveedor")]
        [Range(1, int.MaxValue, ErrorMessage = "El campo {0} es obligatorio.")]
        public int SupplierId { get; set; }

        public IEnumerable<SelectListItem> Suppliers { get; set; }

        [Display(Name = "Vendedor")]
        [Range(1, int.MaxValue, ErrorMessage = "El campo {0} es obligatorio.")]
        public int SupplierContactId { get; set; } //Id de un contacto

        public IEnumerable<SelectListItem> SupllierContacts { get; set; }

        [Display(Name = "Entrega estimada")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? EstimatedDeliveryDate { get; set; }

        [Display(Name = "Fecha entrega")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? DeliveryDate { get; set; }

        [StringLength(128)]
        [Display(Name = "Cotización")]
        public string? SupplierQuote { get; set; }

        [Display(Name = "Estatus")]
        public IEnumerable<SelectListItem> OrderStatus { get; set; }

        [Display(Name = "Estatus")]
        public int OrderStatusId { get; set; }

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
