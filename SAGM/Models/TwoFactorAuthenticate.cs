using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class TwoFactorAuthenticate
    {
        [Display(Name = "Direccion de correo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debes ingresar un correo valido.")]
        public string Username { get; set; }

        //Cadena de caracteres usada para generar su código en Google Authenticator
        public string SecurityStamp { get; set; }

        [Display(Name = "Doble factor de autenticacion")]
        public bool TwoFactorEnabled { get; set; }

        //Digitos que devuelve Google Authenticator para hacer el segundo factor de autenticacion
        [Display(Name = "Código doble factor")]
        public int mfaCode { get; set; }
    }
}
