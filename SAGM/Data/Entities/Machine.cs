using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SAGM.Data.Entities
{
    public class Machine
    {

        [Key]
        [Display(Name = "Id de Maquina")]
        public int MachineId { get; set; }

        [Display(Name = "Nombre Maquina")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string MachineName { get; set; }

        [Display(Name = "Activo")]
        [DefaultValue(true)]
        public Boolean Active { get; set; }

        [JsonIgnore]
        public Process Process { get; set; }

        public ICollection<WorkOrderDetailProcess> WorkOrderDetailsProcesses { get; set; }


    }
}
