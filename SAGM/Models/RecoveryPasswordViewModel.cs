using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace SAGM.Models
{
    public class RecoveryPasswordViewModel
    {

        [Display(Name = "Direccion de correo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debes ingresar un correo valido.")]
        public string Email { get; set; }
    }
}
