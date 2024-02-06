using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAGM.Data.Entities
{
    public class QuoteDetail
    {
        [Key]
        public int QuoteDetailId { get; set; }

        [Display(Name = "Cotización")]
        public Quote Quote { get; set; }

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cantidad")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Price { get; set; }

        public Material Material { get; set; }

        public Unit Unit { get; set; }

       



    }
}
