using Microsoft.AspNetCore.Mvc.Rendering;
using SAGM.Data.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class WorkOrderDetailViewProcessModel
    {
        public WorkOrderDetail WorkOrderDetail { get; set; }

        [Key]
        public int WorkOrderDetailProcessId { get; set; }

    }
}
