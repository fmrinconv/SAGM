using SAGM.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class AllWorkOrders: WorkOrder
    {
        
        public string  BuyerContact { get; set; }
        public string  FinalUser { get; set; }

        [Display(Name = "Nombre de Archivo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [StringLength(200, MinimumLength = 6, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres")]
        public string ArchiveName { get; set; }

        [Display(Name = "OCArchiveGuid")]
        public Guid? OCArchiveGuid { get; set; }


    }
}
