using SAGM.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class AllQuotes: Quote
    {
        
        public string  BuyerContact { get; set; }
        public string  FinalUser { get; set; }

    }
}
