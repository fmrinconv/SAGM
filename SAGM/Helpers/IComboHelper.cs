using Microsoft.AspNetCore.Mvc.Rendering;
using SAGM.Data.Entities;

namespace SAGM.Helpers
{
    public interface IComboHelper
    {
        Task<IEnumerable<SelectListItem>> GetComboCountriesAsync();

        Task<IEnumerable<SelectListItem>> GetComboStatesAsync(int countryId);
        
        Task<IEnumerable<SelectListItem>> GetComboCitiesAsync(int stateId);

        Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync();

        Task<IEnumerable<SelectListItem>> GetComboMaterialTypesAsync(int categoryId);

        Task<IEnumerable<SelectListItem>> GetComboMaterialsAsync(int materialTypeId);
    }
}
