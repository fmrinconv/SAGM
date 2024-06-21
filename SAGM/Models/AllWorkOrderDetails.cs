using Mono.TextTemplating;
using SAGM.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class AllWorkOrderDetails: WorkOrderDetail
    {
        public string MaterialName { get; set; }
        public string UnitName { get; set; }

        public decimal Total => Quantity * Price;
        public ICollection<Archive> Archives { get; set; }

        public ICollection<WorkOrderDetailProcess> Processes { get; set; }

        [Display(Name = "Archivos")]
        public int ArchivesNumber => Archives == null ? 0 : Archives.Count;

        public string ArchivesChain { get; set; }
        public string Processchain { get; set; }

    }
}
