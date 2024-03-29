﻿using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using System.Text.Json.Serialization;

namespace SAGM.Data.Entities
{
    public class MaterialType
    {

        public int MaterialTypeId { get; set; }

        [Display(Name = "Tipo de material")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string MaterialTypeName { get; set; }

        public Boolean Active { get; set; }

        [JsonIgnore]//este dato es bien importante porque puede marcar en el retorno de Json el error 500
        public Category Category { get; set; }

        [JsonIgnore]
        public ICollection<Material> Materials { get; set; }

        [Display(Name = "Materiales")]
        public int MaterialsNumber => Materials == null ? 0 : Materials.Count;

       
    }
}
