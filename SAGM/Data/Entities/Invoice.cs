using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAGM.Data.Entities
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [Display(Name = "serie")]
        [MaxLength(4, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string ? Serie { get; set; } = null;

        [Display(Name = "Folio")]
        public int ? Folio { get; set; } = null;

        [Required]
        [Display(Name = "UUID")]
        [MaxLength(36, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string UUID { get; set; }

        [Display(Name = "Fecha de emisión")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        [Column("Fecha de Emisión")]
        public DateTime? EmisionDate { get; set; } = null;


        [Display(Name = "RFC Emisior")]
        [MaxLength(36, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Column("RFC Emisor")]
        public string? EmisorTaxId { get; set; } = null;


        [Display(Name = "Nombre del Emisor")]
        [MaxLength(36, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Column("Nombre del Emisor")]
        public string? EmisorName { get; set; } = null;

        [Display(Name = "RFC Receptor")]
        [MaxLength(36, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Column("RFC Receptor")]
        public string? ReceptorTaxId { get; set; } = null;

        [Display(Name = "Nombre del Receptor")]
        [MaxLength(160, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Column("Nombre del Receptor")]
        public string? ReceptorName { get; set; } = null;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        public decimal? Subtotal { get; set; } = null;


        [Column("IVA Trasladado", TypeName = "decimal(18,2)")]
        [Display(Name = "IVA Trasladado")]
        public decimal? TrasladedTax { get; set; } = null;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total")]
        public decimal? Total { get; set; } = null;


        [Display(Name = "Moneda")]
        [Column("Moneda")]
        [MaxLength(12, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string? Currency { get; set; } = null;


        [Display(Name = "Estado Fiscal")]
        [MaxLength(32, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Column("Estado Fiscal")]
        public string? TaxStatus { get; set; } = null;

        [Display(Name = "Pagado")]
        [MaxLength(32, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Column("Pagado")]
        public string? PayStatus { get; set; } = null;

        [Display(Name = "Tipo de Comprobante")]
        [MaxLength(32, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Column("Tipo de Comprobante")]
        public string? RecipeType { get; set; } = null;


        [Display(Name = "Tipo de CFDI")]
        [MaxLength(16, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Column("Tipo de CFDI")]
        public string? CfdiType { get; set; } = null;

        [Display(Name = "Fecha de pago")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        [Column("Fecha de pago")]
        public DateTime? PayDate { get; set; } = null;

        [Display(Name = "Comentario de pago")]
        [MaxLength(256, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Column("Comentario de pago")]
        public string? payComment { get; set; } = null;

        [Display(Name = "Metodo de pago")]
        [MaxLength(32, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Column("Metodo de pago")]
        public string? payMethod { get; set; } = null;

        [Display(Name = "Fecha de carga")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        [Column("Fecha de carga")]
        public DateTime? LoadedDate { get; set; } = DateTime.Now;

        [Display(Name = "Tipo de carga")]
        [Column("Tipo de carga")]
        [MaxLength(16, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string? LoadType { get; set; } = "InvoicesTralix";  ///XMLInvoice es el otro tipo de carga


    }
}
