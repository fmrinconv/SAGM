using Microsoft.AspNetCore.Mvc.Rendering;
using SAGM.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class EditSupplierViewModel
    {
        public int SupplierId { get; set; }


        [Display(Name = "Razón Social")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string SupplierName { get; set; }

        [Display(Name = "Cliente")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string SupplierNickName { get; set; }

        [Display(Name = "RFC")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string TaxId { get; set; }

        [Display(Name = "País")]
        [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar un oaís")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public int CountryId { get; set; }

        public IEnumerable<SelectListItem> Countries { get; set; }

        [Display(Name = "Estado")]
        [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar un estado")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public int StateId { get; set; }

        public IEnumerable<SelectListItem> States { get; set; }

        [Display(Name = "Ciudad")]
        [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar una ciudad")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public int CityId { get; set; }

        public IEnumerable<SelectListItem> Cities { get; set; }


        [Display(Name = "Dirección")]
        [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Address { get; set; }

        [Display(Name = "CP")]
        [MaxLength(5, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string PostalCode { get; set; }

        [Display(Name = "Teléfono")]
        [MaxLength(20, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string PhoneNumber { get; set; }


        [Display(Name = "Foto")]
        public Guid ImageId { get; set; }

        public string? httproute { get; set; }

        //TODO Rutas en app
        [Display(Name = "Foto")]
        public string ImageFullPath => ImageId == Guid.Empty
            ? $"https://localhost:44345/images/noimage.png"
            : $"https://almacenamientomanolorin1.blob.core.windows.net/suppliers/{ImageId}";

        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Activo")]
        [DefaultValue(true)]
        public Boolean Active { get; set; }

        [Display(Name = "Dias crédito")]
        public int? CreditDays { get; set; }

    }
}
