using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SAGM.Data.Entities
{
    public class QuoteStatus
    {
        [Key]
        public int QuoteStatusId { get; set; }

        [Display(Name = "Estatus")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string QuoteStatusName { get; set; }
        public ICollection<Quote> Quote { get; set; }
    }
}
