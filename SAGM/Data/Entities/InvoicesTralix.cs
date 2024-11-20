using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SAGM.Data.Entities
{
  
    public class InvoicesTralix
    {
        [Key]
        [Display(Name = "Id Registro")]
        public int InvoiceId { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }
        public string UUID { get; set; }
        public string FechadeEmision { get; set; }
        public string RFCEmisor { get; set; }
        public string NombredelEmisor { get; set; }
        public string RFCReceptor { get; set; }
        public string NombredelReceptor { get; set; }
        public string Subtotal { get; set; }
        public string IVATrasladado { get; set; }
        public string Total { get; set; }
        public string Moneda { get; set; }
        public string EstadoFiscal { get; set; }
        public string Pagado { get; set; }
        public string TipodeComprobante { get; set; }
        public string TipodeCFDI { get; set; }
        public string FechadePago { get; set; }
        public string ComentariodePago { get; set; }
        public string MetododePago { get; set; }
    }
}
