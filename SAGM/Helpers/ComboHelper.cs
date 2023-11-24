using Microsoft.AspNetCore.Mvc.Rendering;
using SAGM.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SAGM.Data.Entities;

namespace SAGM.Helpers
{
    public class ComboHelper : IComboHelper
    {
        private readonly SAGMContext _context;

        public ComboHelper(SAGMContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCountriesAsync()
        {
            List<SelectListItem> list = await _context.Countries.Select(c => new SelectListItem
            {
                Text = c.CountryName,
                Value = c.CountryId.ToString()
            })
                 .OrderBy(c => c.Text)
                 .ToListAsync();

            list.Insert(0, new SelectListItem { Text = "[Seleccione un país...]", Value = "0" });
            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboStatesAsync(int countryId)
        {
            List<SelectListItem> list = await _context.States
                .Where(s => s.Country.CountryId == countryId)
                .Select(s => new SelectListItem
                {
                    Text = s.StateName,
                    Value = s.StateId.ToString()
                })
               .OrderBy(c => c.Text)
               .ToListAsync();

            list.Insert(0, new SelectListItem { Text = "[Seleccione un estado...]", Value = "0" });
            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCitiesAsync(int stateId)
        {
            List<SelectListItem> list = await _context.Cities
               .Where(c => c.State.StateId == stateId)
               .Select(c => new SelectListItem
               {
                   Text = c.CityName,
                   Value = c.CityId.ToString()
               })
              .OrderBy(c => c.Text)
              .ToListAsync();

            list.Insert(0, new SelectListItem { Text = "[Seleccione una ciudad...]", Value = "0" });
            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync()
        {
            List<SelectListItem> list = await _context.Categories.Select(c => new SelectListItem 
            { 
                Text = c.CategoryName,
                Value = c.CategoryId.ToString()
            })
                .OrderBy(c => c.Text)
                .ToListAsync();

            list.Insert(0, new SelectListItem { Text = "[Seleccione una categoría...]", Value = "0" });
            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboMaterialTypesAsync(int categoryId)
        {
            List<SelectListItem> list = await _context.MaterialTypes
               .Where(mt => mt.Category.CategoryId == categoryId)
               .Select(mt => new SelectListItem
               {
                   Text = mt.MaterialTypeName,
                   Value = mt.MaterialTypeId.ToString()
               })
              .OrderBy(c => c.Text)
              .ToListAsync();

            list.Insert(0, new SelectListItem { Text = "[Seleccione un tipo de material...]", Value = "0" });
            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboMaterialsAsync(int materialTypeId)
        {
            List<SelectListItem> list = await _context.Materials
              .Where(m => m.MaterialType.MaterialTypeId == materialTypeId)
              .Select(m => new SelectListItem
              {
                  Text = m.MaterialName,
                  Value = m.MaterialId.ToString()
              })
             .OrderBy(c => c.Text)
             .ToListAsync();

            list.Insert(0, new SelectListItem { Text = "[Seleccione un material...]", Value = "0" });
            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCustomersAsync()
        {
            List<SelectListItem> list = await _context.Customers.Select(c => new SelectListItem
            {
                Text = c.CustomerNickName,
                Value = c.CustomerId.ToString()
            })
                 .OrderBy(c => c.Text)
                 .ToListAsync();

            list.Insert(0, new SelectListItem { Text = "[Seleccione un cliente...]", Value = "0" });
            return list;
        }


        public async Task<IEnumerable<SelectListItem>> GetComboContactCustomersAsync(int customerId)
        {

            List<SelectListItem> list = new List<SelectListItem>();

            if (customerId > 0)
            {
                list = await _context.Contacts
              .Where(c => c.Customer.CustomerId == customerId)
              .Select(c => new SelectListItem
              {
                  Text = c.Name + " " + c.LastName,
                  Value = c.ContactId.ToString()
              })
             .OrderBy(c => c.Text)
             .ToListAsync();
            }
          

            list.Insert(0, new SelectListItem { Text = "[Seleccione un contacto...]", Value = "0" });
            return list;
        }

    }
}
