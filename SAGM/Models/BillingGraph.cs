using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class BillingGraph
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Fecha inicial")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly DateIni { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Fecha final")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly DateFin { get; set; }
    }
}
