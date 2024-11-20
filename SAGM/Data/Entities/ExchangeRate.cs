using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SAGM.Data.Entities
{
    public class ExchangeRate
    {
        [Key]
        [Display(Name = "Id Registro")]
        public int ExhangeRateId { get; set; }


        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Fecha cotización")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly Date { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "IVA")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Exchangerate { get; set; }


    }
}
