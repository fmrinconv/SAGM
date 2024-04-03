
namespace SAGM.Models
{
    public class CopyQuote
    {
        public int QuoteId { get; set; }
        public string QuoteName { get; set; }

        public bool copyfilesheader { get; set; }
        public bool copyfilesdetails { get; set; }
    }
}
