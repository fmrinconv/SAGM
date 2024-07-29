using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class WorkOrderForecastModel
    {
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Display(Name = "Cantidad")]
        [Range(1, double.MaxValue, ErrorMessage = "La cantidad debe ser mayo que cero.")]
        public decimal Total { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "MP")]
        [Range(.01, double.MaxValue, ErrorMessage = "El precio debe ser mayo que cero.")]
        public decimal MaterialCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "MO")]
        [Range(.01, double.MaxValue, ErrorMessage = "El precio debe ser mayo que cero.")]
        public decimal ProcessCost { get; set; }

    }
}
