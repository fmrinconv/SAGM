﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SAGM.Data.Entities
{
    public class WorkOrderComment
    {

        [Key]
        public int CommentId { get; set; }

        [JsonIgnore]
        public WorkOrder WorkOrder { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        [Display(Name = "Comentario")]
        [Required(ErrorMessage = "Debes ingresar el comentario ")]
        public string Comment { get; set; }


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateComment { get; set; }
    }
}
