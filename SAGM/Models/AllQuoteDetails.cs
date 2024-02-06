using SAGM.Data.Entities;

namespace SAGM.Models
{
    public class AllQuoteDetails: QuoteDetail
    {
        public string MaterialName { get; set; }
        public string UnitName { get; set; }

        public decimal Total => Quantity * Price;
    }
}
