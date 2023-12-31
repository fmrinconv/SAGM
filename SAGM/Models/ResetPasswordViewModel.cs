﻿using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace SAGM.Models
{
    public class ResetPasswordViewModel
    {

        [Display(Name = "Direccion de correo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debes ingresar un correo valido.")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres")]
        public string Password { get; set; }


        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmación de contraseña")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres")]
        public string PasswordConfirm { get; set; }


        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Token { get; set; }
    }
}
