using Microsoft.AspNetCore.Mvc.Rendering;
using SAGM.Data.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Newtonsoft.Json;

namespace SAGM.Models
{
    public class AllWorkOrderDeliveries: WorkOrder
    {


        [Display(Name = "Vendedor")]
        public String BuyerContact { get; set; }

        [Display(Name = "Usuario final")]
        public String FinalUser { get; set; }




    }
}
