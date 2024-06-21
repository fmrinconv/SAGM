using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using SAGM.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAGM.Models
{
    public class WorkOrderDetailViewModel
    {
        public int WorkOrderDetailId { get; set; }

        public int WorkOrderId { get; set; }


        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Description { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Display(Name = "Cantidad")]
        [Range(1, double.MaxValue, ErrorMessage = "La cantidad debe ser mayo que cero.")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio")]
        [Range(.01, double.MaxValue, ErrorMessage = "El precio debe ser mayo que cero.")]
        public decimal Price { get; set; }

        public IEnumerable<SelectListItem> Unit { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Unidad de medida")]
        public int UnitId { get; set; }


        [Display(Name = "Categoría")]
        [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar una cateogoría")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public int CategoryId { get; set; }

        public IEnumerable<SelectListItem> Category { get; set; }


        [Display(Name = "Tipo de Material")]
        public IEnumerable<SelectListItem> MaterialType { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Tipo de Material")]
        public int MaterialTypeId { get; set; }


        [Display(Name = "Material")]
        public IEnumerable<SelectListItem> Material { get; set; }

        
        [Range(1, int.MaxValue, ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Material")]
        public int MaterialId { get; set; }

        [Display(Name = "Archivos")]
        public ICollection<Archive> Archives { get; set; }

        [Display(Name = "Archivos")]
        public int ArchivesNumber => Archives == null ? 0 : Archives.Count;


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


    }
}
