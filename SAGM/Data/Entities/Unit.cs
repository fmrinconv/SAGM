using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SAGM.Data.Entities
{
    public class Unit
    {
        [Key]
        public int UnitId { get; set; }

        [Display(Name = "Unidad de medida")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string UnitName { get; set; }

        [Display(Name = "Activa")]
        [Required]
        public Boolean Active { get; set; }

        public ICollection<QuoteDetail> QuoteDetails { get; set; }

    }
}
