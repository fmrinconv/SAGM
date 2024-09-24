using System.ComponentModel.DataAnnotations;

namespace SAGM.Data.Entities
{
    public class OrderReceipt
    {
        [Key]
        public int OrderReceiptId { get; set; }

        public Order Order { get; set; }

        [Required]
        public string ReceiptName { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Fecha cotización")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime ReceiptDate { get; set; }

        [Display(Name = "Comentarios")]
        public string? Comments { get; set; }

        [Display(Name = "Receptor")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public User ReceivedBy { get; set; }

        public ICollection<OrderReceiptDetail> OrderReceiptDetails { get; set; }

    }
}
