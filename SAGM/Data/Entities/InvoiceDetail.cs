using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAGM.Data.Entities
{
    public class InvoiceDetail
    {
        [Key]
        public int InvoiceDetailId { get; set; }

        public int InvoiceId { get; set; }

        public int? Folio { get; set; } = null;

        [Required]
        [Display(Name = "UUID")]
        [MaxLength(36, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string UUID { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Cantidad")]
        public decimal Cantidad { get; set; }

        [Required]
        [Display(Name = "UM")]
        [MaxLength(32, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string UM { get; set; }

        [Required]
        [Display(Name = "Descripcion")]

        public string Description { get; set; }


        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "PU")]
        public decimal PU { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Importe")]
        public decimal Importe { get; set; }
    }
}
