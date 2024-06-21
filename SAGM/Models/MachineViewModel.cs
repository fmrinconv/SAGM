using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class MachineViewModel
    {
        public int MachineId { get; set; }

        [Display(Name = "Maquina")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string MachineName { get; set; }

        [Display(Name = "Activo")]
        [DefaultValue(true)]
        public Boolean Active { get; set; }

        public int ProcessId { get; set; }
    }
}
