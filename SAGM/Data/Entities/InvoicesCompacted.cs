using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAGM.Data.Entities
{
    public class InvoicesCompacted
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "RFC Receptor")]
        [MaxLength(36, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Column("RFC Receptor")]
        public string? ReceptorTaxId { get; set; } = null;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly Date { get; set; }

        [Display(Name = "Día")]
        public int Day { get; set; }

        [Display(Name = "Mes")]
        public int Month { get; set; }

        [Display(Name = "Año")]
        public int Year { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        public decimal? Subtotal { get; set; } = null;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total")]
        public decimal? Total { get; set; } = null;


    }
}
