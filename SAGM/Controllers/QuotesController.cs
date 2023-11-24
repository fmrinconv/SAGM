using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using SAGM.Data;
using SAGM.Data.Entities;
using SAGM.Helpers;
using SAGM.Migrations;
using SAGM.Models;

namespace SAGM.Controllers
{
    public class QuotesController : Controller
    {
        private readonly SAGMContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IComboHelper _comboHelper;

        public QuotesController(SAGMContext context, IUserHelper userHelper, IComboHelper comboHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _comboHelper = comboHelper;
        }

        // GET: Quotes
        public async Task<IActionResult> Index()
        {

            ViewBag.Result = "";
            ViewBag.Message = "";


            if (TempData["AddQuoteResult"] != null)
            {

                ViewBag.Result = TempData["AddQuoteResult"].ToString();
                ViewBag.Message = TempData["AddQuoteMessage"].ToString();
                TempData.Remove("AddQuoteResult");
                TempData.Remove("AddQuoteMessage");
            }
            if (TempData["EditQuoteResult"] != null)
            {

                ViewBag.Result = TempData["EditQuoteResult"].ToString();
                ViewBag.Message = TempData["EditQuoteMessage"].ToString();
                TempData.Remove("EditQuoteResult");
                TempData.Remove("EditQuoteMessage");
            }




            List<Quote> quotes = await _context.Quotes
                  .Include(q => q.Customer)
                  .Include(q => q.QuoteStatus)
                  .ToListAsync();

             List<AllQuotes> lquotes = new List<AllQuotes>();

            foreach (Quote q in quotes)
            {
                string seller = _userHelper.GetUserAsync(q.Seller).Result.FullName.ToString();
                Contact buyercontact = await _context.Contacts.FindAsync(q.BuyerContactId);
                Contact finaluser = await _context.Contacts.FindAsync(q.FinalUserId);

                AllQuotes aqs = new AllQuotes()
                { Active = q.Active,
                  BuyerContact = buyercontact.Name + " " + buyercontact.LastName,
                  Comments = q.Comments,
                  CreatedBy = q.CreatedBy,
                  Currency = q.Currency,
                  Customer = q.Customer,    
                  CustomerPO = q.CustomerPO,
                  FinalUserId = q.FinalUserId,    
                  ModifiedBy = q.ModifiedBy,
                  ModifyDate = q.ModifyDate,
                  QuoteDate = q.QuoteDate, 
                  QuoteDetails = q.QuoteDetails,   
                  QuoteId =  q.QuoteId,
                  QuoteName = q.QuoteName,
                  Seller = seller,
                  Tax = q.Tax,
                  QuoteStatus = q.QuoteStatus,
                  validUntilDate = q.validUntilDate,
                  FinalUser = finaluser.Name + " " + finaluser.LastName,
                };
                lquotes.Add(aqs);
            }

              return View(lquotes.ToList());
        }

        // GET: Quotes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Quotes == null)
            {
                return NotFound();
            }

            var quote = await _context.Quotes
                .FirstOrDefaultAsync(m => m.QuoteId == id);
            if (quote == null)
            {
                return NotFound();
            }

            return View(quote);
        }

        public async Task<IActionResult> AddQuote()
            {
            String Lastnumber = ""; //Ultimo numero consecutivo de la cotización
            int Consec = 0; 

            TimeSpan validuntildate = new TimeSpan(10, 0, 0, 0); //Diez dias de vigencia por defecto

            String quotename = DateTime.Now.ToString("yyyyMMdd"); //Variable formadora de nombre de coti

            quotename = "COT-" + quotename + "-XXX";

           // -------------------------

            Quote Lastquote = await _context.Quotes.OrderBy(q => q.QuoteId).LastOrDefaultAsync();//Ultima cotizacion

            if (Lastquote != null)
            {
                Lastnumber  = Lastquote.QuoteName.Substring(8, 3);
                Consec = Int32.Parse(Lastnumber);
            }
            else {
                Lastnumber = "000";
                Consec = Int32.Parse(Lastnumber);
            }

            //-----------------------

            List<SelectListItem> sellerlist = (List<SelectListItem>) await _userHelper.GetSellersAsync();
            List<SelectListItem> users = new List<SelectListItem>();
            List<SelectListItem> sellers = new List<SelectListItem>();


            users = _context.Users
                   .Where(u => u.EmailConfirmed == true)
                   .Select(u => new SelectListItem
                   {
                       Text = u.FirstName + " " + u.LastName,
                       Value = u.UserName
                   })
                  .OrderBy(c => c.Text)
                  .ToList();

            sellers = (List<SelectListItem>)(from u in users join s in sellerlist on u.Value equals s.Value select u).ToList();

            AddQuote quote = new AddQuote()
            { 
                QuoteName = quotename,
                QuoteDate = DateTime.Now,
                validUntilDate = DateTime.Now.Add(validuntildate),
                Customers = await _comboHelper.GetComboCustomersAsync(),
                CustomerBuyerContacts = await _comboHelper.GetComboContactCustomersAsync(0),
                CustomerFinalContacts = await _comboHelper.GetComboContactCustomersAsync(0),
                Sellers = sellers,
                Tax = 16,
                Active = true,
                ModifiedBy = (List<SelectListItem>)await _userHelper.GetAllUsersAsync(),
            };
 
            return View(quote);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuote(AddQuote model)
                {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.QuoteId == 0)
                    {
                        String quotename = DateTime.Now.ToString("yyyyMMdd"); //Variable formadora de nombre de coti
                        String Lastnumber = ""; //Ultimo numero consecutivo de la cotización
                        String strnumber = "";
                        int Consec = 0;
                        

                        quotename = "COT-" + quotename;

                        // -------------------------

                        Quote Lastquote = await _context.Quotes.Where(q => q.QuoteName.Substring(0,11) == quotename).OrderBy(q => q.QuoteId).LastOrDefaultAsync();//Ultima cotizacion

                        if (Lastquote != null)
                        {
                            Lastnumber = Lastquote.QuoteName.Substring(8, 3);
                            Consec = Int32.Parse(Lastnumber);
                        }
                        else
                        {
                            Lastnumber = "000";
                            Consec = Int32.Parse(Lastnumber);
                        }

                        Lastnumber += 1;

                        strnumber = $"000{Lastnumber}";

                        quotename = quotename + "-" + strnumber.Substring(strnumber.Length - 3,3);

                        //-----------------------
                        Contact buyer = await _context.Contacts.FindAsync(model.BuyerContactId);
                        User createdBy = await _userHelper.GetUserAsync(model.CreatedBy);
                        Currency currency = await _context.Currencies.FindAsync(model.CurrencyId);
                        Customer customer = await _context.Customers.FindAsync(model.CustomerId);
                        QuoteStatus quotestatus = await _context.QuoteStatus.FindAsync(1);//El primer estatus es creada

                        Quote quote = new Quote()
                        {
                            Active = model.Active,
                            BuyerContactId = model.BuyerContactId,
                            Comments = model.Comments,
                            CreatedBy = createdBy,
                            Currency = currency,
                            Customer = customer,
                            CustomerPO = model.CustomerPO,
                            FinalUserId = model.FinalUserId,
                            ModifiedBy = null,
                            ModifyDate = null,
                            QuoteDate = model.QuoteDate,
                            QuoteName = quotename,
                            Seller = model.SellerId,
                            Tax = model.Tax,
                            validUntilDate = model.validUntilDate,
                            QuoteStatus = quotestatus,

                        };
                        _context.Add(quote);
                        await _context.SaveChangesAsync();
                        TempData["AddQuoteResult"] = "true";
                        TempData["AddQuoteMessage"] = "La Cotización fué creada";
                        return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuotes", _context.Quotes.ToList()) });
                    }

                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuotes", _context.Quotes.ToList()) });

                }
                catch (DbUpdateException dbUpdateException)
                {
                  

                    ModelState.AddModelError(string.Empty, dbUpdateException.Message);
                    return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddQuote", model) });

                }

            }
            else
            {

                List<SelectListItem> customers = (List<SelectListItem>)await _comboHelper.GetComboCustomersAsync();
                List<SelectListItem> customerbuyerContacts = new List<SelectListItem>();
                List<SelectListItem> customerfinalContacts = new List<SelectListItem>();
                List<SelectListItem> sellerlist = (List<SelectListItem>)await _userHelper.GetSellersAsync();

                SelectListItem seller = sellerlist.Where(s => s.Value.ToString() == model.SellerId.ToString()).FirstOrDefault();
                seller.Selected = true;

                if (model.CustomerId != 0)
                {
                    var customeritem = customers.Where(c => c.Value.ToString() == model.CustomerId.ToString()).FirstOrDefault();
                    customeritem.Selected = true;

                    customerbuyerContacts = _context.Contacts
                      .Where(c => c.Customer.CustomerId == model.CustomerId)
                      .Select(c => new SelectListItem
                      {
                          Text = c.Name + " " + c.LastName,
                          Value = c.ContactId.ToString()
                      })
                     .OrderBy(c => c.Text)
                     .ToList();
                    customerbuyerContacts.Insert(0, new SelectListItem { Text = "[Seleccione un contacto...]", Value = "0" });


                    customerfinalContacts = _context.Contacts
                      .Where(c => c.Customer.CustomerId == model.CustomerId)
                      .Select(c => new SelectListItem
                      {
                          Text = c.Name + " " + c.LastName,
                          Value = c.ContactId.ToString()
                      })
                     .OrderBy(c => c.Text)
                     .ToList();
                    customerfinalContacts.Insert(0, new SelectListItem { Text = "[Seleccione un contacto...]", Value = "0" });

                    if (model.BuyerContactId != 0)
                    {
                        var buyercontact = customerbuyerContacts.Where(b => b.Value.ToString() == model.BuyerContactId.ToString()).FirstOrDefault();
                        buyercontact.Selected = true;
                    }

                    if (model.FinalUserId != 0)
                    {
                        var finalcontact = customerfinalContacts.Where(b => b.Value.ToString() == model.FinalUserId.ToString()).FirstOrDefault();
                        finalcontact.Selected = true;
                    }
                }
                else {
                    customerbuyerContacts.Insert(0, new SelectListItem { Text = "[Seleccione un contacto...]", Value = "0" });
                    customerfinalContacts.Insert(0, new SelectListItem { Text = "[Seleccione un contacto...]", Value = "0" });
                }


                model.Sellers = sellerlist;
                model.Customers = customers;
                model.CustomerBuyerContacts = customerbuyerContacts;
                model.CustomerFinalContacts = customerfinalContacts;
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddQuote", model) });
            }

        }

        public async Task<IActionResult> EditQuote(int id)
        {


            Quote quote = await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.QuoteStatus)
                .FirstOrDefaultAsync( q => q.QuoteId == id);

            //-----------------------

            List<SelectListItem> sellerlist = (List<SelectListItem>)await _userHelper.GetSellersAsync();
            List<SelectListItem> users = new List<SelectListItem>();
            List<SelectListItem> sellers = new List<SelectListItem>();
            List<SelectListItem> quotestatus = new List<SelectListItem>();



            users = _context.Users
                   .Where(u => u.EmailConfirmed == true)
                   .Select(u => new SelectListItem
                   {
                       Text = u.FirstName + " " + u.LastName,
                       Value = u.UserName
                   })
                  .OrderBy(c => c.Text)
                  .ToList();

            sellers = (List<SelectListItem>)(from u in users join s in sellerlist on u.Value equals s.Value select u).ToList();

            List<SelectListItem> customers = (List<SelectListItem>)await _comboHelper.GetComboCustomersAsync();
            List<SelectListItem> customerbuyercontacts = (List<SelectListItem>)await _comboHelper.GetComboContactCustomersAsync(quote.Customer.CustomerId);
            List<SelectListItem> customerfinalcontacts = (List<SelectListItem>)await _comboHelper.GetComboContactCustomersAsync(quote.Customer.CustomerId);

            quotestatus = _context.QuoteStatus
                .Select(q => new SelectListItem { 
                    Text = q.QuoteStatusName,
                    Value = q.QuoteStatusId.ToString()
                }).OrderBy(c => c.Value).ToList();


            EditQuote editquote = new EditQuote()
            {
                QuoteId = quote.QuoteId,
                QuoteName = quote.QuoteName,
                QuoteDate = quote.QuoteDate,
                validUntilDate = quote.validUntilDate,
                Customers = customers,
                CustomerId = quote.Customer.CustomerId,
                CustomerBuyerContacts = customerbuyercontacts,
                BuyerContactId = quote.BuyerContactId,
                CustomerFinalContacts = customerfinalcontacts,
                FinalUserId = quote.FinalUserId,
                Sellers = sellers,
                Tax = quote.Tax,
                Active = quote.Active,
                ModifiedBy = (List<SelectListItem>)await _userHelper.GetAllUsersAsync(),
                QuoteStatus = quotestatus,
                QuoteStatusId = quote.QuoteStatus.QuoteStatusId
               
            };

            return View(editquote);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuote(EditQuote model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                        Customer customer = await _context.Customers.FindAsync(model.CustomerId);
                        Quote quote = _context.Quotes.Find(model.QuoteId);
                        QuoteStatus quotestatus = _context.QuoteStatus.Find(model.QuoteStatusId);

                        quote.Active = model.Active;
                        quote.BuyerContactId = model.BuyerContactId;
                        quote.Comments = model.Comments;
                        quote.Customer = customer;
                        quote.CustomerPO = model.CustomerPO;
                        quote.FinalUserId = model.FinalUserId;
                        quote.ModifiedBy = User.Identity.Name;
                        quote.ModifyDate = DateTime.Now;
                        quote.QuoteDate = model.QuoteDate;
                        quote.Seller = model.SellerId;
                        quote.QuoteName = model.QuoteName;
                        quote.Tax = model.Tax;
                        quote.validUntilDate = model.validUntilDate;
                        quote.CreatedBy = await _userHelper.GetUserAsync(model.CreatedBy);
                        quote.QuoteStatus = quotestatus;
                        _context.Update(quote);
                        await _context.SaveChangesAsync();
                        TempData["EditQuoteResult"] = "true";
                        TempData["EditQuoteMessage"] = "La Cotización fué actualizada";
                        return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuotes", _context.Quotes.ToList()) });

                }
                catch (DbUpdateException dbUpdateException)
                {


                    ModelState.AddModelError(string.Empty, dbUpdateException.Message);
                    return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditQuote", model) });

                }

            }
            else
            {
                
                //-----------------------

                List<SelectListItem> sellerlist = (List<SelectListItem>)await _userHelper.GetSellersAsync();
                List<SelectListItem> users = new List<SelectListItem>();
                List<SelectListItem> sellers = new List<SelectListItem>();


                users = _context.Users
                       .Where(u => u.EmailConfirmed == true)
                       .Select(u => new SelectListItem
                       {
                           Text = u.FirstName + " " + u.LastName,
                           Value = u.UserName
                       })
                      .OrderBy(c => c.Text)
                      .ToList();

                sellers = (List<SelectListItem>)(from u in users join s in sellerlist on u.Value equals s.Value select u).ToList();

                List<SelectListItem> customers = (List<SelectListItem>)await _comboHelper.GetComboCustomersAsync();
                List<SelectListItem> customerbuyercontacts = (List<SelectListItem>)await _comboHelper.GetComboContactCustomersAsync(model.CustomerId);
                List<SelectListItem> customerfinalcontacts = (List<SelectListItem>)await _comboHelper.GetComboContactCustomersAsync(model.CustomerId);


                EditQuote editquote = new EditQuote()
                {
                    QuoteName = model.QuoteName,
                    QuoteDate = model.QuoteDate,
                    validUntilDate = model.validUntilDate,
                    Customers = customers,
                    CustomerId = model.CustomerId,
                    CustomerBuyerContacts = customerbuyercontacts,
                    BuyerContactId = model.BuyerContactId,
                    CustomerFinalContacts = customerfinalcontacts,
                    FinalUserId = model.FinalUserId,
                    Sellers = sellers,
                    Tax = model.Tax,
                    Active = true,
                    ModifiedBy = (List<SelectListItem>)await _userHelper.GetAllUsersAsync(),
                };

                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditQuote", model) });
            }

        }


        // GET: Quotes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Quotes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( AddQuote model)
        {
            Contact buyer = await _context.Contacts.FindAsync(model.BuyerContactId);
            User createdBy = await _userHelper.GetUserAsync(model.CreatedBy);
            Currency currency = await _context.Currencies.FindAsync(model.CurrencyId);  
            Customer customer = await _context.Customers.FindAsync(model.CustomerId);


            Quote quote = new Quote()
            {
                 Active = model.Active,

                 Comments = model.Comments,
                 CreatedBy = createdBy,
                 Currency = currency,
                 Customer  = customer,
                 CustomerPO = model.CustomerPO,
                 FinalUserId = model.FinalUserId,
                 ModifiedBy = null,
                 ModifyDate = model.ModifyDate,
                 QuoteDate = model.QuoteDate,
                 QuoteName = model.QuoteName,

                 Tax = model.Tax,  
                 validUntilDate = model.validUntilDate

            };

            if (ModelState.IsValid)
            {
                _context.Add(quote);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(quote);
        }

        // GET: Quotes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Quotes == null)
            {
                return NotFound();
            }

            var quote = await _context.Quotes.FindAsync(id);
            if (quote == null)
            {
                return NotFound();
            }
            return View(quote);
        }

        // POST: Quotes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("QuoteId,QuoteDate,QuoteName,Seller,FinalUserId,validUntilDate,ModifyDate,ModifiedBy,CustomerPO,Comments,Active,Tax")] Quote quote)
        {
            if (id != quote.QuoteId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quote);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuoteExists(quote.QuoteId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(quote);
        }

        // GET: Quotes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Quotes == null)
            {
                return NotFound();
            }

            var quote = await _context.Quotes
                .FirstOrDefaultAsync(m => m.QuoteId == id);
            if (quote == null)
            {
                return NotFound();
            }

            return View(quote);
        }

        // POST: Quotes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Quotes == null)
            {
                return Problem("Entity set 'SAGMContext.Quotes'  is null.");
            }
            var quote = await _context.Quotes.FindAsync(id);
            if (quote != null)
            {
                _context.Quotes.Remove(quote);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public JsonResult GetBuyerContacts(int customerId)
        {

            List<SelectListItem> Contacts = new List<SelectListItem>();

            Contacts =  _context.Contacts
                   .Where(c => c.Customer.CustomerId == customerId)
                   .Select(c => new SelectListItem
                   {
                       Text = c.Name + " " + c.LastName,
                       Value = c.ContactId.ToString()
                   })
                  .OrderBy(c => c.Text)
                  .ToList();

            return Json(Contacts );

        }

        private bool QuoteExists(int id)
        {
          return _context.Quotes.Any(e => e.QuoteId == id);
        }
    }
}
