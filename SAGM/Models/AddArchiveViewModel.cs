using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SAGM.Data.Entities;

namespace SAGM.Models
{
    public class AddArchiveViewModel: Archive
    {


        [Display(Name = "Selecciona archivo(s) a cargar")] 
        public IFormFile? ArchiveFile { get; set; }

        public Controller Controller { get; set; }
    }
}
