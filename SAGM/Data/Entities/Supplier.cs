using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SAGM.Data.Entities
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Display(Name = "Razon Social")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string SupplierName { get; set; }

        [Display(Name = "Proveedor")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string SupplierNickName { get; set; }

        [Display(Name = "RFC")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string TaxId { get; set; }


        [Display(Name = "Ciudad")]
        public City City { get; set; }

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

        //TODO Rutas en app
        [Display(Name = "Logo")]
        public string ImageFullPath => ImageId == Guid.Empty
            ? $"https://localhost:44345/images/noimage.png"
            : $"https://almacenamientomanolorin1.blob.core.windows.net/suppliers/{ImageId}";

        [Display(Name = "Activo")]
        [DefaultValue(true)]
        public Boolean Active { get; set; }

        [Display(Name = "Dias crédito")]
        public int? CreditDays { get; set; }

        public ICollection<Contact> Contacts { get; set; }

        public ICollection<Order> Orders { get; set; }

    }
}
