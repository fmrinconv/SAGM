
namespace SAGM.Models
{
    public class CopyOrder
    {
        public int OrderId { get; set; }
        public string OrderName { get; set; }

        public bool copyfilesheader { get; set; }
        public bool copyfilesdetails { get; set; }
    }
}
