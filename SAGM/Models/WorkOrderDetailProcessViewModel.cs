using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Rendering;
using SAGM.Data.Entities;
namespace SAGM.Models
{
    public class WorkOrderDetailProcessViewModel
    {
        public int WorkOrderDetailProcessId { get; set; }

        [Display(Name = "Partida")]
        public int WorkOrderDetailId { get; set; }

        [Display(Name = "Procesos")]
        [JsonIgnore]
        public int ProcessId { get; set; }

        public IEnumerable<SelectListItem> Process { get; set; }

        [Display(Name = "Maquinas")]
        [JsonIgnore]
        public int MachineId { get; set; }

        public IEnumerable<SelectListItem> Machines { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cantidad")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Quantity { get; set; }

        [Display(Name = "Unidad")]
        public int UnitId { get; set; }

        [Display(Name = "Unidades")]
        public IEnumerable<SelectListItem> Units { get; set; }


    }
}




