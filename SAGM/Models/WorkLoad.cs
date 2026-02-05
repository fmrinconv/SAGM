using SAGM.Data.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
namespace SAGM.Models
{
    public class WorkLoad
    {
        public int WorkOrderDetailId { get; set; }

        [Display(Name = "Descripción")]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cantidad")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total")]
        public decimal Total => Quantity == null || Price == null ? 0 : Quantity*Price;

        [Display(Name = "Materia prima")]
        public decimal RawMaterial { get; set; } = 0;

        [Display(Name = "Maquinado")]
        public decimal Machined { get; set; } = 0;

        [Display(Name = "Tratamiento")]
        public decimal TT { get; set; } = 0;

        [Display(Name = "Embarcado")]
        public decimal Shipped { get; set; } = 0;

        [Display(Name = "Facturado")]
        public decimal Invoiced { get; set; } = 0;

        public bool Completed { get; set; } = false;
        public string MaterialName { get; set; }
        public string UnitName { get; set; }

        [Display(Name = "OTid")]
        public int WorkOrderId { get; set; }

        [Display(Name = "Quoteid")]
        public int QuoteId { get; set; }

        [Display(Name = "OT")]
        public string WorkOrderName { get; set; }

        [Display(Name = "Cotización")]
        public string QuoteName { get; set; }

        [Display(Name = "OT Estatus")]
        public string WorkOrderStatusName { get; set; }

        [Display(Name = "Cliente")]
        public string CustomerNickName { get; set; }

        [Display(Name = "OC")]
        public string CustomerPO { get; set; }
        [Display(Name = "Requerimiento")]
        public string CustomerRFQ { get; set; }

        [Display(Name = "Usuario Final")]
        public string FinalUser { get; set; }

        [Display(Name = "Comprador")]
        public string Buyer { get; set; }

        [Display(Name = "Vendedor")]
        public string Seller { get; set; }

        [Display(Name = "Fecha OT")]
        public DateTime PODate { get; set; }

        [Display(Name = "Fecha compromiso")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? PromiseDate { get; set; }

        [Display(Name = "Fecha Embarque")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? ShippedDate { get; set; }

        [Display(Name = "Moneda")]
        public string Currency { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tipo cambio")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal ExchangeRate { get; set; }

        [Display(Name = "Procesos")]
        public string ProcessArray { get; set; }

        [Display(Name = "OT Active")]
        public bool Active { get; set; }



    }
}
