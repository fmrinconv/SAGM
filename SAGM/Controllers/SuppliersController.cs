using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SAGM.Data;
using SAGM.Data.Entities;
using SAGM.Enums;
using SAGM.Helpers;
using SAGM.Models;
using static SAGM.Helpers.ModalHelper;

namespace SAGM.Controllers
{
    [Authorize(Roles = "Administrador,Comprador")]
    public class SuppliersController : Controller
    {
        private readonly SAGMContext _context;
        private readonly IComboHelper _comboHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IConfiguration _configuration;
        private string hostpath;

        public SuppliersController(SAGMContext context, IComboHelper comboHelper, IBlobHelper blobHelper, IConfiguration configuration)
        {
            _context = context;
            _comboHelper = comboHelper;
            _blobHelper = blobHelper;
            _configuration = configuration;
            hostpath = _configuration["UrlPath:path"] + "images/noimage.png";
         
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Result = "";
            ViewBag.Message = "";

            if (TempData["SupplierDeleteResult"] != null)
            {
                ViewBag.Result = TempData["SupplierDeleteResult"].ToString();
                ViewBag.Message = TempData["SupplierDeleteMessage"].ToString();
                TempData.Remove("SupplierDeleteResult");
                TempData.Remove("SupplierDeleteMessage");
            }

            if (TempData["AddOrEditSupplierResult"] != null)
            {
                ViewBag.Result = TempData["AddOrEditSupplierResult"].ToString();
                ViewBag.Message = TempData["AddOrEditSupplierMessage"].ToString();
                TempData.Remove("AddOrEditSupplierResult");
                TempData.Remove("AddOrEditSupplierMessage");
            }

            return View(await _context.Suppliers
                .ToListAsync());

        }


        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                EditSupplierViewModel model = new()
                {
                    Countries = await _comboHelper.GetComboCountriesAsync(),
                    States = await _comboHelper.GetComboStatesAsync(0),
                    Cities = await _comboHelper.GetComboCitiesAsync(0),

                };
                return View(model);

            }
            else
            {


                Supplier supplier = await _context.Suppliers
                    .Include(s => s.City)
                    .ThenInclude(s => s.State)
                    .ThenInclude(s => s.Country)
                    .FirstOrDefaultAsync(s => s.SupplierId == id);

                

                EditSupplierViewModel model = new()
                {
                    SupplierId = supplier.SupplierId,
                    SupplierName = supplier.SupplierName,
                    SupplierNickName = supplier.SupplierNickName,
                    TaxId = supplier.TaxId,
                    Address = supplier.Address,
                    CityId = supplier.City.CityId,
                    StateId = supplier.City.State.StateId,
                    CountryId = supplier.City.State.Country.CountryId,
                    Cities = await _comboHelper.GetComboCitiesAsync(supplier.City.State.StateId),
                    States = await _comboHelper.GetComboStatesAsync(supplier.City.State.Country.CountryId),
                    Countries = await _comboHelper.GetComboCountriesAsync(),
                    ImageId = supplier.ImageId,
                    Active = supplier.Active,
                    CreditDays = supplier.CreditDays,
                    PostalCode = supplier.PostalCode,
                    PhoneNumber = supplier.PhoneNumber,

                };
                return View(model);
            }

            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(EditSupplierViewModel model)
        {
            Guid imageid = Guid.Empty;
            if (model.ImageFile != null)
            {
                if (model.ImageId != imageid)
                {
                    await _blobHelper.DeleteBlobAsync(model.ImageId, "suppliers");
                }


                imageid = await _blobHelper.UploadBlobAsync(model.ImageFile, "suppliers");
                model.ImageId = imageid;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (model.SupplierId == 0)
                    {
                        City city = await _context.Cities.FindAsync(model.CityId);

                        Supplier supplier = new() { 
                            SupplierName = model.SupplierName,
                            SupplierNickName = model.SupplierNickName,
                            TaxId = model.TaxId,
                            City = city,
                            Address = model.Address,
                            PostalCode = model.PostalCode,
                            PhoneNumber = model.PhoneNumber,
                            ImageId = model.ImageId,
                            Active = model.Active,
                            CreditDays = model.CreditDays,
                            

                        };
                        _context.Add(supplier);
                        await _context.SaveChangesAsync();
                        TempData["AddOrEditSupplierResult"] = "true";
                        TempData["AddOrEditSupplierMessage"] = "El cliente fué agregado";
                    }
                    else
                    {
                        Supplier supplier = _context.Suppliers.Find(model.SupplierId);
                        supplier.SupplierName = model.SupplierName;
                        supplier.SupplierNickName = model.SupplierNickName;
                        supplier.TaxId = model.TaxId;
                        supplier.City = await _context.Cities.FindAsync(model.CityId).ConfigureAwait(false);
                        supplier.Address = model.Address;
                        supplier.PostalCode = model.PostalCode;
                        supplier.PhoneNumber = model.PhoneNumber;
                        supplier.ImageId = model.ImageId;
                        supplier.Active = model.Active;
                        supplier.CreditDays = model.CreditDays;
                        _context.Update(supplier);
                        await _context.SaveChangesAsync();
                        TempData["AddOrEditSupplierResult"] = "true";
                        TempData["AddOrEditSupplierMessage"] = "El cliente fué actualizado";
                    }
                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllSuppliers", _context.Categories.ToList()) });

                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe un cliente con el mismo nombre");
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEdit", model) });
                    }
                    else
                    {
                        TempData["AddOrEditSupplierResult"] = "false";
                        TempData["AddOrEditSupplierMessage"] = dbUpdateException.InnerException.Message;
                    }

                }

                catch (Exception exception)
                {
                    TempData["AddOrEditSupplierResult"] = "false";
                    TempData["AddOrEditSupplierResult"] = exception.Message;

                }
                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllSuppliers", _context.Categories.ToList()) });

            }
            else
            {
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEditContact", model) });
            }

        }

        public JsonResult GetStates(int countryId)
        {
            Country country = _context.Countries
                .Include(c => c.States)
                .FirstOrDefault(c => c.CountryId == countryId);
            if (country == null)
            {
                return null;
            }

            return Json(country.States.OrderBy(s => s.StateName));
        }

        public JsonResult GetCities(int stateId)
        {
            State state = _context.States
                .Include(s => s.Cities)
                .FirstOrDefault(s => s.StateId == stateId);
            if (state == null)
            {
                return null;
            }

            return Json(state.Cities.OrderBy(c => c.CityName));
        }


        // GET: Suppliers

        // GET: Suppliers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewBag.Result = "";
            ViewBag.Message = "";

            if (TempData["AddOrEditContactResult"] != null)
            {

                ViewBag.Result = TempData["AddOrEditContactResult"].ToString();
                ViewBag.Message = TempData["AddOrEditContactMessage"].ToString();
                TempData.Remove("AddOrEditContactResult");
                TempData.Remove("AddOrEditContactResult");
            }

            if (TempData["DeleteContactResult"] != null)
            {

                ViewBag.Result = TempData["DeleteContactResult"].ToString();
                ViewBag.Message = TempData["DeleteContactMessage"].ToString();
                TempData.Remove("DeleteContactResult");
                TempData.Remove("DeleteContactMessage");
            }
           

            if (id == null || _context.Suppliers == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(m => m.SupplierId == id);
            if (supplier == null)
            {
                return NotFound();
            }

            

            IEnumerable<Contact> contacts = _context.Contacts.Where(c => c.Supplier.SupplierId == id)
                .Include(c => c.Supplier)
                .ToList();
            List<ContactViewModel> contactsvm = new List<ContactViewModel>().ToList();

            foreach (var item in contacts)
            {
                if (item.ImageId == Guid.Empty)
                {
               
                   
                };
                ContactViewModel cvm = new()
                {
                    Active = item.Active,
                    ContactId = item.ContactId,
                    Email = item.Email,
                    ImageFile = item.ImageFile,
                    ImageId = item.ImageId,
                    LastName = item.LastName,
                    Mobile = item.Mobile,
                    Name = item.Name,
                    PhoneNumber = item.PhoneNumber,
                    Supplier = item.Supplier

                };
                contactsvm.Add(cvm);
            }

            SupplierViewModel cust = new SupplierViewModel();
            cust.Active = supplier.Active;
            cust.Address = supplier.Address;
            cust.City = supplier.City;
            cust.ContactsVM = contactsvm.ToList();
            cust.SupplierNickName = supplier.SupplierNickName;
            cust.CreditDays = supplier.CreditDays;
            cust.SupplierId = supplier.SupplierId;
            cust.SupplierName = supplier.SupplierName;
            cust.SupplierNickName = supplier?.SupplierNickName;
            cust.ImageId = supplier.ImageId;
            cust.PhoneNumber = supplier.PhoneNumber;    
            cust.PostalCode = supplier.PostalCode;
            cust.TaxId = supplier.TaxId;

            return View(cust);
        }


        public async Task<IActionResult> DeleteSupplier(int? id)
        {

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(c => c.SupplierId == id);
            try
            {
                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
                TempData["SupplierDeleteResult"] = "true";
                TempData["SupplierDeleteMessage"] = "El cliente fué eliminado";
            }

            catch (Exception e)
            {
                TempData["SupplierDeleteResult"] = "false";
                TempData["SupplierDeleteMessage"] = "El cliente no pudo ser eliminado ya que tiene registros de contactos ligados " + e.Message;
                return RedirectToAction(nameof(Index));

            }

            return RedirectToAction(nameof(Index));

        }

        private bool SupplierExists(int id)
        {
            return _context.Suppliers.Any(e => e.SupplierId == id);
        }

        public async Task<IActionResult> AddOrEditContact(int id , int contactId = 0)
        {
            Supplier supplier = await _context.Suppliers.FirstOrDefaultAsync(c => c.SupplierId == id);
            if (contactId == 0)
            {
                Contact model = new()
                {
                    Supplier = supplier,
                    Active = true,
                };
                return View(model);

            }
            else
            {

                Contact model = await _context.Contacts
                    .Include(c => c.Supplier)
                    .FirstOrDefaultAsync(c => c.ContactId == contactId);
                return View(model);
            }


        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEditContact(Contact model)
        {
            Supplier supplier = await _context.Suppliers
                .FirstOrDefaultAsync(c => c.SupplierId == model.Supplier.SupplierId);
            model.Supplier = supplier;
            if (ModelState.IsValid)
            {

                Guid imageid = Guid.Empty;

                if (model.ImageFile != null)
                {
                    if (model.ImageId != imageid)
                    {
                        await _blobHelper.DeleteBlobAsync(model.ImageId, "contacts");
                    }


                    imageid = await _blobHelper.UploadBlobAsync(model.ImageFile, "contacts");
                    model.ImageId = imageid;
                }

                try
                {
                    if (model.ContactId == 0)
                    {
                   
                        Contact contact = new()
                        {
                            Name = model.Name,
                            LastName = model.LastName,
                            Email = model.Email,
                            PhoneNumber = model.PhoneNumber,
                            Mobile = model.Mobile,
                            ImageId = model.ImageId,
                            Active = model.Active,
                            Supplier = model.Supplier
                        };
                        _context.Add(contact);
                        await _context.SaveChangesAsync();
                        TempData["AddOrEditContactResult"] = "true";
                        TempData["AddOrEditContactMessage"] = "El contacto fué agregado";
                    
                    }
                    else
                    {
                        Contact contact = new()
                        {
                            ContactId = model.ContactId,
                            Name = model.Name,
                            LastName = model.LastName,
                            Email = model.Email,
                            PhoneNumber = model.PhoneNumber,
                            Mobile = model.Mobile,
                            ImageId = model.ImageId,
                            Active = model.Active
                        };
                        _context.Update(contact);
                        await _context.SaveChangesAsync();
                        TempData["AddOrEditContactResult"] = "true";
                        TempData["AddOrEditContactMessage"] = "El contacto fué actualizado";

                    }
                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "Details", supplier.SupplierId) });

                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe un contacto con el mismo nombre");
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEditContact", model) });
                    }
                    else
                    {
                        TempData["AddOrEditContactResult"] = "false";
                        TempData["AddOrEditContactMessage"] = dbUpdateException.InnerException.Message;
                    }

                }

                catch (Exception exception)
                {
                    TempData["AddOrEditContactResult"] = "false";
                    TempData["AddOrEditContactResult"] = exception.Message;

                }
                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllContacts", _context.Suppliers.ToList()) });

            }
            else
            {
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEditContact", model) });
            }

        }

        [NoDirectAccess]
        public async Task<IActionResult> DeleteContact(int? id)
        {

            Contact contact = await _context.Contacts
                .Include(c => c.Supplier)
                .FirstOrDefaultAsync(c => c.ContactId == id);
            int SupplierId = contact.Supplier.SupplierId;
            try
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
                TempData["DeleteContactResult"] = "true";
                TempData["DeleteContactMessage"] = "El contacto fué eliminado";
            }

            catch
            {
                TempData["DeleteContactResult"] = "false";
                TempData["DeleteContactMessage"] = "El contacto no pudo ser eliminado";
                return RedirectToAction(nameof(Details), new { id = SupplierId });

            }

            return RedirectToAction(nameof(Details), new { id = SupplierId });

        }
    }
}
