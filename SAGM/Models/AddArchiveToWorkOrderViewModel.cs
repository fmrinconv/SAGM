using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SAGM.Data.Entities;

namespace SAGM.Models
{
    public class AddArchiveToWorkOrderViewModel: Archive
    {


        [Display(Name = "Selecciona archivo a cargar")] 
        public IFormFile? ArchiveFile { get; set; }

        public Controller Controller { get; set; }

        [Display(Name = "Es Orden de compra")]
        [DefaultValue(true)]
        public Boolean isPurchaseOrder { get; set; }

        public string CustomerPO { get; set; } = null;

    }
}
