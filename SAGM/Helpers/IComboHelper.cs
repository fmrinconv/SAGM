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

        Task<IEnumerable<SelectListItem>> GetComboCustomersAsync();

        Task<IEnumerable<SelectListItem>> GetComboSuppliersAsync();

        Task<IEnumerable<SelectListItem>> GetComboContactCustomersAsync(int customerId);

        Task<IEnumerable<SelectListItem>> GetComboContactSuppliersAsync(int supplierId);

        Task<IEnumerable<SelectListItem>> GetComboUnitAsync(int selectedindex = 0);

        Task<IEnumerable<SelectListItem>> GetComboOrderStatus(int selectedindex = 0);

        Task<IEnumerable<SelectListItem>> GetComboQuoteStatus(int selectedindex = 0);
        Task<IEnumerable<SelectListItem>> GetComboWorkOrderStatus(int selectedindex = 0);

        Task<IEnumerable<SelectListItem>> GetComboCurrenciesAsync(int selectedindex = 0);

        Task<IEnumerable<SelectListItem>> GetComboProcessAsync(int selectedindex = 0);

        Task<IEnumerable<SelectListItem>> GetComboMachinesAsync(int selectedindex = 0);


    }
}
