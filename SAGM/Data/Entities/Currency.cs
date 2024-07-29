using System.ComponentModel.DataAnnotations;

namespace SAGM.Data.Entities
{
    public class Currency
    {
        [Key]
        public int CurrencyId { get; set; }

        [Display(Name = "Moneda")]
        [MaxLength(3, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Curr { get; set; }

        public ICollection<Quote> Quotes { get; set; }
        public ICollection<Order> Orders { get; set; }

    }
}
