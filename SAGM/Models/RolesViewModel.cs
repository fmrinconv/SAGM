using System.ComponentModel.DataAnnotations;

namespace SAGM.Models
{
    public class RolesViewModel
    {
        [Display(Name = "Role")]
        public string Name { get; set; }

    }
}
