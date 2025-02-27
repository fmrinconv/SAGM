using System.ComponentModel.DataAnnotations;

namespace SAGM.Data.Entities
{
    public class Sat_TipoComprobante
    {
        [Key]
        public int TipoComprobanteId { get; set; }


        [Display(Name = "TipoComprobante")]
        [MaxLength(1, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string TipoComprobante { get; set; } = null;

        [Display(Name = "TipoComprobanteDesc")]
        public string TipoComprobanteDesc { get; set; } = null;
    }
}
