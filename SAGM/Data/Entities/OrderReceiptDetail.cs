using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAGM.Data.Entities
{
    public class OrderReceiptDetail
    {
        [Key]
        public int OrderDetailReceiptId { get; set; }

        [Display(Name = "Recibo")]
        public OrderReceipt OrderReceipt { get; set; }

        [Display(Name = "Partida")]
        public OrderDetail OrderDetail { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cantidad")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Quantity { get; set; }


    }
}
