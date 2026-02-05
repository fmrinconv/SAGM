using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class AllWorkOrderArchive: AllWorkOrders
    {

        public string Entity { get; set; } /*Tabla o entidad con la que se va a ligar*/
        public int EntityId { get; set; } /*Id del registro de la entidad*/


        [Display(Name = "Selecciona archivo a cargar")]
        public IFormFile? ArchiveFile { get; set; }

        public Controller Controller { get; set; }

        [Display(Name = "Es Orden de compra")]
        [DefaultValue(true)]
        public Boolean isPurchaseOrder { get; set; }


    }
}
