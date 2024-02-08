using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SAGM.Data;
using SAGM.Data.Entities;
using SAGM.Helpers;
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

            if (TempData["AddOrEditCommentResult"] != null)
            {
                ViewBag.Result = TempData["AddOrEditCommentResult"].ToString();
                ViewBag.Message = TempData["AddOrEditCommentMessage"].ToString();
                TempData.Remove("AddOrEditCommentResult");
                TempData.Remove("AddOrEditCommentMessage");
            }

            if (TempData["DeleteCommentResult"] != null)
            {
                ViewBag.Result = TempData["DeleteCommentResult"].ToString();
                ViewBag.Message = TempData["DeleteCommentMessage"].ToString();
                TempData.Remove("DeleteCommentResult");
                TempData.Remove("DeleteCommentMessage");
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
                .Include(q => q.Customer)
                .Include(q => q.QuoteStatus)
                .Include(q => q.QuoteDetails).ThenInclude(d => d.Unit)
                .Include(q => q.QuoteDetails).ThenInclude(d => d.Material)
                .FirstOrDefaultAsync(m => m.QuoteId == id);
            if (quote == null)
            {
                return NotFound();
            }

            User seller = await _userHelper.GetUserAsync(quote.Seller);

            QuoteViewModel quotev = new QuoteViewModel();
            Contact buyer = await _context.Contacts.FindAsync(quote.BuyerContactId);
            Contact finaluser = await _context.Contacts.FindAsync(quote.FinalUserId);
            quotev.QuoteId = quote.QuoteId;
            quotev.Active = quote.Active;
            quotev.BuyerContactId = quote.BuyerContactId;
            quotev.BuyerName = $"{buyer.Name} {buyer.LastName}"; 
            quotev.Comments = quote.Comments;
            quotev.CreatedBy = quote.CreatedBy;
            quotev.Currency = quote.Currency;
            quotev.Customer = quote.Customer;
            quotev.CustomerPO = quote.CustomerPO;
            quotev.FinalUserId = quote.FinalUserId;
            quotev.FinalUserName = $"{finaluser.Name} {finaluser.LastName}";
            quotev.ModifiedBy = quote.ModifiedBy;
            quotev.ModifyDate = quote.ModifyDate;
            quotev.QuoteComments= quote.QuoteComments;
            quotev.Seller = quote.Seller;
            quotev.SellerName = seller.FullName;
            quotev.QuoteName = quote.QuoteName;
            quotev.QuoteStatus = quote.QuoteStatus;
      
            List<AllQuoteDetails> details = new List<AllQuoteDetails>().ToList();
            if (quote.QuoteDetails != null) {
  
                foreach (var detail in quote.QuoteDetails)
                {
                    AllQuoteDetails detailsv = new()
                    {
                        QuoteDetailId = detail.QuoteDetailId,
                        Quote = detail.Quote,
                        Description = detail.Description,
                        Material = detail.Material,
                        MaterialName = detail.Material.MaterialName,
                        Price = detail.Price,
                        Quantity = detail.Quantity,
                        Unit = detail.Unit,
                        UnitName = detail.Unit.UnitName

                    };
                    details.Add(detailsv);
                }
                
            }

            quotev.QuoteDetails = details;
            


            return View(quotev);
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
                        User createdBy = await _userHelper.GetUserAsync(User.Identity.Name);
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
                CustomerPO = quote.CustomerPO,
                Sellers = sellers,
                Tax = quote.Tax,
                Active = quote.Active,
                ModifiedBy = (List<SelectListItem>)await _userHelper.GetAllUsersAsync(),
                ModifiedById = quote.ModifiedBy,
                QuoteStatus = quotestatus,
                QuoteStatusId = quote.QuoteStatus.QuoteStatusId,
                ModifyDate = quote.ModifyDate,
               
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
                        QuoteStatus quotestatus = _context.QuoteStatus.Find(2);//Estatus 2 es en modificación

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


        public  ActionResult AddOrEditComment(int id)
            {
            List<QuoteCommentView> comments = _context.QuoteComments
                .Include(c => c.User)
                .Where(c => c.Quote.QuoteId == id)
                .Select(c => new QuoteCommentView
                {
                    UserName = c.User.Email,
                    CommentId = c.CommentId,
                    QuoteId = c.Quote.QuoteId,
                    Comment = c.Comment,
                    Usuario = c.User.FullName,
                    DateComment = c.DateComment
                })
                .OrderBy(c => c.QuoteId)
                .ToList();

            ViewBag.QuoteId = id;
            var name = User.Identity.Name;
            User usr =  _context.Users.Where(u => u.UserName == name).FirstOrDefault();
            ViewBag.UserName = name;
            ViewBag.Usuario = usr.FullName;
            //ViewBag.UserName = "@" + name.Substring(0, name.IndexOf("@"));



            return View(comments);


        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEditComment(int QuoteId, QuoteCommentView model)
        {
            if (ModelState.IsValid)
            {

                try
                {

                        QuoteComment comment = new QuoteComment()
                        {
                            Quote = await _context.Quotes.FindAsync(QuoteId),
                            Comment = model.Comment,
                            DateComment = DateTime.Now,
                            User = await _userHelper.GetUserAsync(User.Identity.Name)
                        };
                 
                        _context.Add(comment);
                        await _context.SaveChangesAsync();
                        TempData["AddOrEditCommentResult"] = "true";
                        TempData["AddOrEditCommentMessage"] = "El comentario fué agregado";

                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuotes", _context.Quotes.ToList()) });

                }
                catch (DbUpdateException dbUpdateException)
                {
               
                        ModelState.AddModelError(string.Empty, dbUpdateException.Message);
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEditMessage", model) });
  
                }

            }
            else
            {
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrdEditMessage", model) });
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id)
        {
            QuoteComment comment = await _context.QuoteComments.FindAsync(id);
            try
            {
                _context.QuoteComments.Remove(comment);
                await _context.SaveChangesAsync();
                TempData["DeleteCommentResult"] = "true";
                TempData["DeleteCommentMessage"] = "El comentario fué eliminado";
                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuotes", _context.Quotes.ToList()) });
      

            }
            catch (Exception e)
            {
                TempData["DeleteCommentResult"] = "false";
                TempData["EeleteCommentMessage"] = "El comentario no pudo ser eliminado error: " + e.Message;
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuotes", _context.Quotes.ToList()) });

            }


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


        public async Task<IActionResult> AddOrEditDetail(int id = 0, int detailid = 0)
        {
            //El Detalle siempre traerá un id de cotización ya que sea nuevo el detalle o actualización la Quote ya existe
            QuoteDetailViewModel qdv = new QuoteDetailViewModel();
            //Buscamos la cotización para pasar todos sus valores al Detalle
            qdv.QuoteId = id;
            qdv.QuoteDetailId = detailid;

            //Pasamos el listado de unidades para que aparezca en un combo despues asignaremos un default si el 
            //movimiento es un Edit si es un nuevo detalle entonces se quedeará como esta sin asignar el id de la unidad
            List<SelectListItem> Unit = (List<SelectListItem>)await _comboHelper.GetComboUnitAsync();
            qdv.Unit = Unit;

            List<SelectListItem> Category = (List<SelectListItem>)await _comboHelper.GetComboCategoriesAsync();

            qdv.Category = Category;

            List<SelectListItem> MaterialType = (List<SelectListItem>)await _comboHelper.GetComboMaterialTypesAsync(0);

            if (detailid == 0)
            {
              
                return View(qdv);
            }
            else
            {
                Category category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound();
                }
                return View(qdv);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddOrEditDetail(QuoteDetailViewModel model)
        {

      
           
            if (ModelState.IsValid)
            {
                try
                {
                    QuoteDetail qd = new QuoteDetail();
                    qd.Quote = await _context.Quotes.FindAsync(model.QuoteId); ;
                    qd.Description = model.Description;
                    qd.Material = await _context.Materials.FindAsync(model.MaterialId);
                    qd.Price = model.Price;
                    qd.Quantity = model.Quantity;
                    qd.Unit = await _context.Units.FindAsync(model.UnitId);
                    if (model.QuoteDetailId != 0)
                    {
                        _context.Update(qd);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        _context.Add(qd);
                        await _context.SaveChangesAsync();
                    }

                    /*
                      Nos traemos todos los detalles

                     */
                    List<QuoteDetail> list = new List<QuoteDetail>();
                    list = _context.Quotes.FirstOrDefault(x => x.QuoteId == model.QuoteId).QuoteDetails.ToList();
                    List<AllQuoteDetails> details = new List<AllQuoteDetails>().ToList();

                        foreach (var detail in list)
                        {
                            AllQuoteDetails detailsv = new()
                            {
                                QuoteDetailId = detail.QuoteDetailId,
                                Quote = detail.Quote,
                                Description = detail.Description,
                                Material = detail.Material,
                                MaterialName = detail.Material.MaterialName,
                                Price = detail.Price,
                                Quantity = detail.Quantity,
                                Unit = detail.Unit,
                                UnitName = detail.Unit.UnitName

                            };
                            details.Add(detailsv);
                        }

                    
                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuoteDetails", details) });
                }
                catch (Exception)
                {

                    return View(model);

                }
               
               
            }
            else {

                {
                    List<SelectListItem> Unit = (List<SelectListItem>)await _comboHelper.GetComboUnitAsync();
                    model.Unit = Unit;

                    List<SelectListItem> Category = (List<SelectListItem>)await _comboHelper.GetComboCategoriesAsync();
                    model.Category = Category;

                    List<SelectListItem> MaterialType = (List<SelectListItem>)await _comboHelper.GetComboMaterialTypesAsync(model.CategoryId);
                    model.MaterialType = MaterialType;

                    List<SelectListItem> Material = (List<SelectListItem>)await _comboHelper.GetComboMaterialsAsync(model.MaterialTypeId);
                    model.Material = Material;

                    return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEditDetail", model) });
                }
            }
            
        }

        public JsonResult GetMaterialTypes(int categoryId)
        {
            Category category = _context.Categories
                .Include(c => c.MaterialTypes)
                .FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                return null;
            }

            return Json(category.MaterialTypes.OrderBy(m => m.MaterialTypeName));
        }

        public JsonResult GetMaterials(int materialTypeId)
        {
            MaterialType materialtype = _context.MaterialTypes
              .Include(s => s.Materials)
              .FirstOrDefault(s => s.MaterialTypeId == materialTypeId);
            if (materialtype == null)
            {
                return null;
            }
            return Json(materialtype.Materials.OrderBy(m => m.MaterialName));
        }


        private bool QuoteExists(int id)
        {
          return _context.Quotes.Any(e => e.QuoteId == id);
        }
    }
}
