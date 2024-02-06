using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace SAGM.Data.Entities
{
    public class Archive
    {
        public int ArchiveId { get; set; }

        [Display(Name = "Archivo")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string ArchiveName { get; set; }

        [Display(Name = "Archivo")]
        public Guid ImageId { get; set; }

        // Rutas en app
        [Display(Name = "Archivo")]
        public string ImageFullPath => ImageId == Guid.Empty
            ? $"https://localhost:7060/images/noimage.png"
            : $"https://almacenamientomanolorin1.blob.core.windows.net/users/{ImageId}";
    }
}
