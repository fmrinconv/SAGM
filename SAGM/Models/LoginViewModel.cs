using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace SAGM.Models
{
    public class LoginViewModel
    {

      
        [Display(Name = "Direccion de correo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debes ingresar un correo valido.")]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MinLength(6, ErrorMessage = "El campo {0} debe tener almenos {1} caracteres")]
        public string Password { get; set; }

        [Display(Name = "Recordarme en este navegador")]
        public bool RememberMe { get; set; }

        [Display(Name = "Correo confirmado")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Doble factor de autenticacion")]
        public bool TwoFactorEnabled { get; set; }

        [Display(Name = "Código doble factor")]
        public int mfaCode { get; set; }
    }
}
