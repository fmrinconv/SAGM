using SAGM.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class AllOrders: Order
    {
        
        public string  SupplierContact { get; set; }

    }
}
