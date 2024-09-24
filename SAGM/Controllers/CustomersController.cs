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
    public class CustomersController : Controller
    {
        private readonly SAGMContext _context;
        private readonly IComboHelper _comboHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IConfiguration _configuration;
        private string hostpath;

        public CustomersController(SAGMContext context, IComboHelper comboHelper, IBlobHelper blobHelper, IConfiguration configuration)
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

            if (TempData["CustomerDeleteResult"] != null)
            {
                ViewBag.Result = TempData["CustomerDeleteResult"].ToString();
                ViewBag.Message = TempData["CustomerDeleteMessage"].ToString();
                TempData.Remove("CustomerDeleteResult");
                TempData.Remove("CustomerDeleteMessage");
            }

            if (TempData["AddOrEditCustomerResult"] != null)
            {
                ViewBag.Result = TempData["AddOrEditCustomerResult"].ToString();
                ViewBag.Message = TempData["AddOrEditCustomerMessage"].ToString();
                TempData.Remove("AddOrEditCustomerResult");
                TempData.Remove("AddOrEditCustomerMessage");
            }

            return View(await _context.Customers.OrderBy(c => c.CustomerNickName)
                .ToListAsync());

        }


        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                EditCustomerViewModel model = new()
                {
                    Countries = await _comboHelper.GetComboCountriesAsync(),
                    States = await _comboHelper.GetComboStatesAsync(0),
                    Cities = await _comboHelper.GetComboCitiesAsync(0),

                };
                return View(model);

            }
            else
            {


                Customer customer = await _context.Customers
                    .Include(c => c.City)
                    .ThenInclude(c => c.State)
                    .ThenInclude(c => c.Country)
                    .FirstOrDefaultAsync(c => c.CustomerId == id);

                

                EditCustomerViewModel model = new()
                {
                    CustomerId = customer.CustomerId,
                    CustomerName = customer.CustomerName,
                    CustomerNickName = customer.CustomerNickName,
                    TaxId = customer.TaxId,
                    Address = customer.Address,
                    CityId = customer.City.CityId,
                    StateId = customer.City.State.StateId,
                    CountryId = customer.City.State.Country.CountryId,
                    Cities = await _comboHelper.GetComboCitiesAsync(customer.City.State.StateId),
                    States = await _comboHelper.GetComboStatesAsync(customer.City.State.Country.CountryId),
                    Countries = await _comboHelper.GetComboCountriesAsync(),
                    ImageId = customer.ImageId,
                    Active = customer.Active,
                    CreditDays = customer.CreditDays,
                    PostalCode = customer.PostalCode,
                    PhoneNumber = customer.PhoneNumber,

                };
                return View(model);
            }

            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(EditCustomerViewModel model)
        {
            Guid imageid = Guid.Empty;
            if (model.ImageFile != null)
            {
                if (model.ImageId != imageid)
                {
                    await _blobHelper.DeleteBlobAsync(model.ImageId, "customers");
                }


                imageid = await _blobHelper.UploadBlobAsync(model.ImageFile, "customers");
                model.ImageId = imageid;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (model.CustomerId == 0)
                    {
                        City city = await _context.Cities.FindAsync(model.CityId);

                        Customer customer = new() { 
                            CustomerName = model.CustomerName,
                            CustomerNickName = model.CustomerNickName,
                            TaxId = model.TaxId,
                            City = city,
                            Address = model.Address,
                            PostalCode = model.PostalCode,
                            PhoneNumber = model.PhoneNumber,
                            ImageId = model.ImageId,
                            Active = model.Active,
                            CreditDays = model.CreditDays,
                            

                        };
                        _context.Add(customer);
                        await _context.SaveChangesAsync();
                        TempData["AddOrEditCustomerResult"] = "true";
                        TempData["AddOrEditCustomerMessage"] = "El cliente fué agregado";
                    }
                    else
                    {
                        Customer customer = _context.Customers.Find(model.CustomerId);
                        customer.CustomerName = model.CustomerName;
                        customer.CustomerNickName = model.CustomerNickName;
                        customer.TaxId = model.TaxId;
                        customer.City = await _context.Cities.FindAsync(model.CityId).ConfigureAwait(false);
                        customer.Address = model.Address;
                        customer.PostalCode = model.PostalCode;
                        customer.PhoneNumber = model.PhoneNumber;
                        customer.ImageId = model.ImageId;
                        customer.Active = model.Active;
                        customer.CreditDays = model.CreditDays;
                        _context.Update(customer);
                        await _context.SaveChangesAsync();
                        TempData["AddOrEditCustomerResult"] = "true";
                        TempData["AddOrEditCustomerMessage"] = "El cliente fué actualizado";
                    }
                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllCustomers", _context.Categories.ToList()) });

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
                        TempData["AddOrEditCustomerResult"] = "false";
                        TempData["AddOrEditCustomerMessage"] = dbUpdateException.InnerException.Message;
                    }

                }

                catch (Exception exception)
                {
                    TempData["AddOrEditCustomerResult"] = "false";
                    TempData["AddOrEditCustomerResult"] = exception.Message;

                }
                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllCustomers", _context.Categories.ToList()) });

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


        // GET: Customers

        // GET: Customers/Details/5
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
           

            if (id == null || _context.Customers == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            

            IEnumerable<Contact> contacts = _context.Contacts.Where(c => c.Customer.CustomerId == id)
                .Include(c => c.Customer)
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
                    Customer = item.Customer

                };
                contactsvm.Add(cvm);
            }

            CustomerViewModel cust = new CustomerViewModel();
            cust.Active = customer.Active;
            cust.Address = customer.Address;
            cust.City = customer.City;
            cust.ContactsVM = contactsvm.ToList();
            cust.CustomerNickName = customer.CustomerNickName;
            cust.CreditDays = customer.CreditDays;
            cust.CustomerId = customer.CustomerId;
            cust.CustomerName = customer.CustomerName;
            cust.CustomerNickName = customer?.CustomerNickName;
            cust.ImageId = customer.ImageId;
            cust.PhoneNumber = customer.PhoneNumber;    
            cust.PostalCode = customer.PostalCode;
            cust.TaxId = customer.TaxId;

            return View(cust);
        }


        public async Task<IActionResult> DeleteCustomer(int? id)
        {

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == id);
            try
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["CustomerDeleteResult"] = "true";
                TempData["CustomerDeleteMessage"] = "El cliente fué eliminado";
            }

            catch (Exception e)
            {
                TempData["CustomerDeleteResult"] = "false";
                TempData["CustomerDeleteMessage"] = "El cliente no pudo ser eliminado ya que tiene registros de contactos ligados " + e.Message;
                return RedirectToAction(nameof(Index));

            }

            return RedirectToAction(nameof(Index));

        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }

        public async Task<IActionResult> AddOrEditContact(int id , int contactId = 0)
        {
            Customer customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
            if (contactId == 0)
            {
                Contact model = new()
                {
                    Customer = customer,
                    Active = true,
                };
                return View(model);

            }
            else
            {

                Contact model = await _context.Contacts
                    .Include(c => c.Customer)
                    .FirstOrDefaultAsync(c => c.ContactId == contactId);
                return View(model);
            }


        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEditContact(Contact model)
        {
            Customer customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == model.Customer.CustomerId);
            model.Customer = customer;
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
                            Customer = model.Customer
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
                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "Details", customer.CustomerId) });

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
                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllContacts", _context.Customers.ToList()) });

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
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.ContactId == id);
            int CustomerId = contact.Customer.CustomerId;
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
                return RedirectToAction(nameof(Details), new { id = CustomerId });

            }

            return RedirectToAction(nameof(Details), new { id = CustomerId });

        }
    }
}
