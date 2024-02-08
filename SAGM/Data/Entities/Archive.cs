using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace SAGM.Data.Entities
{
    public class Archive
    {
        public int ArchiveId { get; set; }

        [Display(Name = "Archivo")]
        public IFormFile? ArchiveName { get; set; }

        [Display(Name = "Archivo")]
        public Guid ArchiveGuid { get; set; }

        // Rutas en app
        [Display(Name = "Archivo")]
        public string ImageFullPath => ArchiveGuid == Guid.Empty
            ? $"https://localhost:7060/images/noimage.png"
            : $"https://almacenamientomanolorin1.blob.core.windows.net/archives/{ArchiveGuid}";
    }
}
