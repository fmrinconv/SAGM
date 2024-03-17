using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SAGM.Enums;
using System.Text.Json.Serialization;
namespace SAGM.Data.Entities
{
    public class Archive
    {
        public int ArchiveId { get; set; }
        public string Entity { get; set; } /*Tabla o entidad con la que se va a ligar*/
        public int EntityId { get; set; } 

        [Display(Name = "Nombre de Archivo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [StringLength(200, MinimumLength = 6, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caracteres")]
        public string ArchiveName { get; set; }

        [Display(Name = "Archivo")]
        public Guid ArchiveGuid { get; set; }

        // Rutas en app
        [Display(Name = "Archivo")]
        public string ArchiveFullPath => ArchiveGuid == Guid.Empty
            ? $"https://localhost:7060/images/noimage.png"
            : $"https://almacenamientomanolorin1.blob.core.windows.net/archives/{ArchiveGuid}";

    

    }
}
