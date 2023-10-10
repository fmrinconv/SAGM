using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Xml.Linq;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAGM.Data.Entities
{
    public class Contact
    {
        [Key]
        public int ContactId { get; set; }

        [Display(Name = "Nombre")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Name { get; set; }

        [Display(Name = "Apellido")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string LastName { get; set; }


        [EmailAddress]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Email { get; set; }

        [Display(Name = "Teléfono")]
        [MaxLength(20, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Movil")]
        [MaxLength(20, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string? Mobile { get; set; }

        [Display(Name = "Foto")]
        public Guid ImageId { get; set; }

        [Display(Name = "Image")]
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        //TODO Rutas en app
        [Display(Name = "Foto")]
        public string ImageFullPath => ImageId == Guid.Empty
            ? $"https://localhost:7060/images/noimage.png"
            : $"https://almacenamientomanolorin1.blob.core.windows.net/contacts/{ImageId}";

        [Display(Name = "Activo")]
        [DefaultValue(true)]
        public Boolean Active { get; set; }

        [Display(Name = "Contacto")]
        public string FullName => $"{Name} {LastName}";


        public Customer Customer { get; set; }
    }
}
