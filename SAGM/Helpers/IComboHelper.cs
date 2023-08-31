using Microsoft.AspNetCore.Mvc.Rendering;
using SAGM.Data.Entities;

namespace SAGM.Helpers
{
    public interface IComboHelper
    {
        Task<IEnumerable<SelectListItem>> GetComboCountriesAsync();
    }
}
