using System.ComponentModel.DataAnnotations.Schema;

namespace SAGM.Models
{
    public class GraphicPay
    {

        public string x { get; set; } //Va a traer Nombre corto de Cliente, nombre de archivo Blob  ejemplo NGK|032d04ef-1525-4b6c-852f-2e68e84f8bf7


        [Column(TypeName = "decimal(18,2)")]
        public decimal? value { get; set; } =null;
    }
}
