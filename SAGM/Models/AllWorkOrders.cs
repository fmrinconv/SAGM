using SAGM.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class AllWorkOrders: WorkOrder
    {
        
        public string  BuyerContact { get; set; }
        public string  FinalUser { get; set; }


    }
}
