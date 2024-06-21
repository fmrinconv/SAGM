using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SAGM.Data.Entities
{
    public class Process
    {
        [Key]

        [Display(Name = "Número proceso")]
        public int ProcessId { get; set; }

        [Display(Name = "Proceso")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string ProcessName { get; set; }

        [Display(Name = "Activo")]
        [DefaultValue(true)]
        public Boolean Active { get; set; }

        public ICollection<Machine> Machines { get; set; }

        [Display(Name = "Cantidad de Maquinas")]
        public int MachineNumber => Machines == null ? 0 : Machines.Count;

     
    }
}
