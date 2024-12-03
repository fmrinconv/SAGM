using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SAGM.Data;
using SAGM.Data.Entities;
using SAGM.Helpers;
using SAGM.Models;
using Azure.Storage.Blobs;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Drawing.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Text;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.IO;
using static SAGM.Helpers.ModalHelper;

using System.Data;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;


namespace SAGM.Controllers
{
    [Authorize(Roles = "Administrador,Vendedor")]
    public class QuotesController : Controller
    {
        private readonly SAGMContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IComboHelper _comboHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IConfiguration _configuration;
        private readonly IReportHelper _reportHelper;

        public QuotesController(SAGMContext context, IUserHelper userHelper, IComboHelper comboHelper, IBlobHelper blobHelper, IConfiguration configuration, IReportHelper reportHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _comboHelper = comboHelper;
            _blobHelper = blobHelper;   
            _configuration = configuration;
            _reportHelper = reportHelper;
        }

        // GET: Quotes

   
        public FileResult DownloadFile(Guid id, string filename)
        {

            string connectionstring = _configuration["Blob:ConnectionString"];
            BlobClient blobClient = new BlobClient(connectionstring, "archives", id.ToString());
            using (var stream = new MemoryStream())
            {
                blobClient.DownloadTo(stream);
                stream.Position = 0;
                var contentType = (blobClient.GetProperties()).Value.ContentType;

                FileContentResult archivo = File(stream.ToArray(), contentType, filename);

                return File(stream.ToArray(), contentType, filename);


            }

        
        }


        public  IActionResult Index()
        {

            ViewBag.Result = "";
            ViewBag.Message = "";

            
            ViewBag.DateIni = DateOnly.FromDateTime(DateTime.Now).AddMonths(-1).ToString("yyyy-MM-dd");
            ViewBag.DateFin = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");




            if (TempData["AddQuoteResult"] != null)
            {

                ViewBag.Result = TempData["AddQuoteResult"].ToString();
                ViewBag.Message = TempData["AddQuoteMessage"].ToString();
                TempData.Remove("AddQuoteResult");
                TempData.Remove("AddQuoteMessage");
            };

            if (TempData["EditQuoteResult"] != null)
            {

                ViewBag.Result = TempData["EditQuoteResult"].ToString();
                ViewBag.Message = TempData["EditQuoteMessage"].ToString();
                TempData.Remove("EditQuoteResult");
                TempData.Remove("EditQuoteMessage");
            };

            if (TempData["AddOrEditCommentResult"] != null)
            {
                ViewBag.Result = TempData["AddOrEditCommentResult"].ToString();
                ViewBag.Message = TempData["AddOrEditCommentMessage"].ToString();
                TempData.Remove("AddOrEditCommentResult");
                TempData.Remove("AddOrEditCommentMessage");
            };

            if (TempData["DeleteCommentResult"] != null)
            {
                ViewBag.Result = TempData["DeleteCommentResult"].ToString();
                ViewBag.Message = TempData["DeleteCommentMessage"].ToString();
                TempData.Remove("DeleteCommentResult");
                TempData.Remove("DeleteCommentMessage");
            };
            if (TempData["AddArchiveResult"] != null)
            {

                ViewBag.Result = TempData["AddArchiveResult"].ToString();
                ViewBag.Message = TempData["AddArchiveMessage"].ToString();
                TempData.Remove("AddArchiveResult");
                TempData.Remove("AddArchiveMessage");
            }

            if (TempData["ArchiveDeleteResult"] != null)
            {
                ViewBag.Result = TempData["ArchiveDeleteResult"].ToString();
                ViewBag.Message = TempData["ArchiveDeleteMessage"].ToString();
                TempData.Remove("ArchiveDeleteResult");
                TempData.Remove("ArchiveDeleteMessage");
            }

            if (TempData["CreateWorkOrderResult"] != null)
            {
                ViewBag.Result = TempData["CreateWorkOrderResult"].ToString();
                ViewBag.Message = TempData["CreateWorkOrderMessage"].ToString();
                TempData.Remove("CreateWorkOrderResult");
                TempData.Remove("CreateWorkOrderMessage");
            }

            return View();
              
        }


        [HttpGet]
        public async Task<JsonResult> GetQuotes(DateTime fini, DateTime ffin)
        {
            List<Quote> quotes = await _context.Quotes
                 .Include(q => q.QuoteDetails)
                 .ThenInclude(d => d.Material)
                 .Include(q => q.Customer)
                 .Include(q => q.QuoteStatus)
                 .Include(q => q.Currency)
                 .Include(q => q.CreatedBy)
                 .Where(q => q.QuoteDate >= fini && q.QuoteDate <= ffin.AddHours(23).AddMinutes(59).AddSeconds(59))
                 .OrderByDescending(q => q.QuoteId)
                 .ToListAsync();

            List<QuoteViewIndexModel> lquotes = new List<QuoteViewIndexModel>();

            foreach (Quote q in quotes)
            {
                string seller = _userHelper.GetUserAsync(q.Seller).Result.FullName.ToString();
                Contact buyercontact = await _context.Contacts.FindAsync(q.BuyerContactId);
                Contact finaluser = await _context.Contacts.FindAsync(q.FinalUserId);
                int archivesnumber = 0;


            //Armamos la lista de detalles
               List<QuoteDetailViewIndexModel> details = new List<QuoteDetailViewIndexModel>();
                foreach (QuoteDetail qd in q.QuoteDetails)
                {
                    List<Archive> archives = _context.Archives.Where(a => a.Entity == "QuoteDetail" && a.EntityId == qd.QuoteDetailId).ToList();
                    archivesnumber += archives.Count;
                    QuoteDetailViewIndexModel qdvim = new QuoteDetailViewIndexModel()
                    {
                        Quantity = qd.Quantity,
                        Material = _context.Materials.FindAsync(qd.Material.MaterialId).Result.MaterialName.ToString(),
                        Description = qd.Description,
                        Price = qd.Price
                    };
                    details.Add(qdvim); 

                }

                //

                List<Archive> qarchives = _context.Archives.Where(a => a.Entity == "Quote" && a.EntityId == q.QuoteId).ToList();

                string archiveschain = "";

                foreach (var item in qarchives)
                {
                    archiveschain += item.ArchiveGuid.ToString() + "," + item.ArchiveName + "," + item.ArchiveId + "|";
                }
                if (archiveschain != "")
                {
                    archiveschain = archiveschain.Substring(0, archiveschain.Length - 1);
                };

                QuoteViewIndexModel aqs = new QuoteViewIndexModel()
                {
                    QuoteId = q.QuoteId,
                    QuoteDate = q.QuoteDate,
                    Active = q.Active,
                    BuyerContact = $"{buyercontact.Name} {buyercontact.LastName}",
                    Comments = q.Comments,
                    CreatedBy = q.CreatedBy.FullName,
                    Currency = q.Currency.Curr,
                    CustomerNickName = q.Customer.CustomerNickName,
                    CustomerPO = q.CustomerPO,
                    FinalUser = $"{finaluser.Name} {finaluser.LastName}",
                    ModifiedBy = q.ModifiedBy,
                    ModifyDate = q.ModifyDate,
                    QuoteDetails = details,
                    QuoteName = q.QuoteName,
                    Seller = seller,
                    Tax = q.Tax,
                    Discount = q.Discount,
                    QuoteStatusName = q.QuoteStatus.QuoteStatusName,
                    validUntilDate = q.validUntilDate,
                    ArchivesNumber = archivesnumber,
                    ArchivesChain = archiveschain

                };
                lquotes.Add(aqs);
            }
 
            return Json(new {data= lquotes});
        }


        public async Task<List<Quote>> GetDataQuotes()
        {
            List<Quote> quotes =await _context.Quotes
                .Include(q => q.QuoteDetails)
                .ThenInclude(d => d.Material)
                .Include(q => q.Customer)
                .Include(q => q.QuoteStatus)
                .OrderByDescending(q => q.QuoteId)
                .ToListAsync();
            return quotes;
        }

        public async Task<FileResult> Export()
        {

            DataTable dt = new DataTable("QuoteResult");
            dt.Columns.AddRange(new DataColumn[14] { 
                                            new DataColumn("Cotización",Type.GetType("System.String")),
                                            new DataColumn("Cliente",Type.GetType("System.String")),
                                            new DataColumn("Usuario",Type.GetType("System.String")),
                                            new DataColumn("Activa",Type.GetType("System.Boolean")),
                                            new DataColumn("Comprador"),
                                            new DataColumn("Creó"),
                                            new DataColumn("Moneda"),
                                            new DataColumn("OC-Cliente"),
                                            new DataColumn("Vendedor"),
                                            new DataColumn("Material"),
                                            new DataColumn("Descripción"),
                                            new DataColumn("Cantidad",Type.GetType("System.Decimal")),
                                            new DataColumn("Unidad"),
                                            new DataColumn("Precio",Type.GetType("System.Decimal"))
                                            });

            List<Quote> quotes = await _context.Quotes
               .Include(q => q.QuoteDetails).ThenInclude(d => d.Material)
               .Include(q => q.QuoteDetails).ThenInclude(d => d.Unit)
               .Include(q => q.Customer)
               .Include(q => q.QuoteStatus)
               .Include(q => q.Currency)
               .OrderByDescending(q => q.QuoteId)
               .ToListAsync();

            foreach (Quote quote in quotes)
            {
                string seller = _userHelper.GetUserAsync(quote.Seller).Result.FullName.ToString();
                Contact buyercontact =  _context.Contacts.Find(quote.BuyerContactId);
                Contact finaluser = _context.Contacts.Find(quote.FinalUserId);
                foreach (QuoteDetail qd in quote.QuoteDetails)
                {
                    dt.Rows.Add(quote.QuoteName,
                                quote.Customer.CustomerNickName,
                                $"{finaluser.Name} {finaluser.LastName}",
                                quote.Active.ToString(),
                                $"{buyercontact.Name} {buyercontact.LastName}",
                                quote.CreatedBy,
                                quote.Currency.Curr,
                                quote.CustomerPO,
                                quote.Seller,
                                qd.Material.MaterialName,
                                qd.Description,
                                qd.Quantity,
                                qd.Unit.UnitName,
                                qd.Price
                                ); ;
                }
            }

            try
            {
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dt,"Cotizaciones");
                    using (MemoryStream stream = new MemoryStream())
                    {
                        string exportfilename = $"Quote{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", exportfilename);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

        }


        // GET: Quotes/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id, int? quoteDetilId)
            {


            ViewBag.Result = "";
            ViewBag.Message = "";
            ViewBag.quoteDetailId = quoteDetilId.ToString();




            if (TempData != null) {
                if (TempData["AddOrEditQuoteDetailResult"] != null)
                {

                    ViewBag.Result = TempData["AddOrEditQuoteDetailResult"].ToString();
                    ViewBag.Message = TempData["AddOrEditQuoteDetailMessage"].ToString();
                    ViewBag.quoteDetailId = quoteDetilId.ToString();
                    TempData.Remove("AddOrEditQuoteDetailResult");
                    TempData.Remove("AddOrEditQuoteDetailMessage");
                }
                if (TempData["AddArchiveResult"] != null)
                {

                    ViewBag.Result = TempData["AddArchiveResult"].ToString();
                    ViewBag.Message = TempData["AddArchiveMessage"].ToString();
                    TempData.Remove("AddArchiveResult");
                    TempData.Remove("AddArchiveMessage");
                }

                if (TempData["ArchiveDeleteResult"] != null)
                {
                    ViewBag.Result = TempData["ArchiveDeleteResult"].ToString();
                    ViewBag.Message = TempData["ArchiveDeleteMessage"].ToString();
                    TempData.Remove("ArchiveDeleteResult");
                    TempData.Remove("ArchiveDeleteMessage");
                }
                if (TempData["DeleteQuoteDetailtResult"] != null)
                {
                    ViewBag.Result = TempData["DeleteQuoteDetailtResult"].ToString();
                    ViewBag.Message = TempData["DeleteQuoteDetailMessage"].ToString();
                    TempData.Remove("DeleteQuoteDetailtResult");
                    TempData.Remove("DeleteQuoteDetailMessage");
                }


            }


            if (id == null || _context.Quotes == null)
            {
                return NotFound();
            }

            var quote = await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.QuoteDetails)
                .Include(q => q.QuoteStatus)
                .Include(q => q.Currency)
                .FirstOrDefaultAsync(m => m.QuoteId == id);

            ViewBag.DetailsCount = quote.QuoteDetails.Count();

            if (quote == null)
            {
                return NotFound();
            }

            List<SelectListItem> quotestatus = (List<SelectListItem>)await _comboHelper.GetComboQuoteStatus(quote.QuoteStatus.QuoteStatusId);
           

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
            quotev.QuoteStatus = quotestatus;
            quotev.QuoteStatusId = quote.QuoteStatus.QuoteStatusId;
            quotev.Tax = quote.Tax;
            quotev.ExchangeRate = quote.ExchangeRate;
            quotev.Discount = quote.Discount;

            return View(quotev);    
           
        }

        [HttpGet]
        public async Task<JsonResult> GetQuoteDetails(int id)
        {

            var quote = await _context.Quotes
                .Include(q => q.QuoteDetails).ThenInclude(d => d.Unit)
                .Include(q => q.QuoteDetails).ThenInclude(d => d.Material)
                .Include(q => q.QuoteDetails).ThenInclude(d => d.QuoteDetailComments)
                .FirstOrDefaultAsync(m => m.QuoteId == id);
            if (quote == null)
            {
                return Json(new {  });
            }

            User seller = await _userHelper.GetUserAsync(quote.Seller);

            QuoteViewModel quotev = new QuoteViewModel();


            List<AllQuoteDetails> details = new List<AllQuoteDetails>().ToList();
            if (quote.QuoteDetails != null)
            {

                foreach (var detail in quote.QuoteDetails)
                {

                    List<Archive> archives = _context.Archives.Where(a => a.Entity == "QuoteDetail" && a.EntityId == detail.QuoteDetailId).ToList();

                    string archiveschain = "";

                    foreach (var item in archives)
                    {
                        archiveschain += item.ArchiveGuid.ToString() + "," + item.ArchiveName + "," + item.ArchiveId + "|";
                    }
                    if (archiveschain != "")
                    {
                        archiveschain = archiveschain.Substring(0, archiveschain.Length - 1);
                    };
                   

                    AllQuoteDetails detailsv = new()
                    {
                        QuoteDetailId = detail.QuoteDetailId,
                        Description = detail.Description,
                        MaterialName = detail.Material.MaterialName,
                        Price = detail.Price,
                        Quantity = detail.Quantity,
                        UnitName = detail.Unit.UnitName,
                        ArchivesChain = archiveschain,
                        Archives = archives,
                        QuoteDetailComments = detail.QuoteDetailComments
                    };


                    details.Add(detailsv);
                }

            }

          
            return Json(new { data = details });
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
            List<SelectListItem> currencies = (List<SelectListItem>)await _comboHelper.GetComboCurrenciesAsync(1);

            AddQuote quote = new AddQuote()
            { 
                QuoteName = quotename,
                QuoteDate = DateTime.Now,
                validUntilDate = DateTime.Now.Add(validuntildate),
                Customers = await _comboHelper.GetComboCustomersAsync(),
                CustomerBuyerContacts = await _comboHelper.GetComboContactCustomersAsync(0),
                CustomerFinalContacts = await _comboHelper.GetComboContactCustomersAsync(0),
                Sellers = sellers,
                SellerId = User.Identity.Name,
                Tax = 16,
                Active = true,
                ModifiedBy = (List<SelectListItem>)await _userHelper.GetAllUsersAsync(),
                ModifiedById = User.Identity.Name,
                ExchangeRate = 1,
                CurrencyId = 1,
                Currency = currencies
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

                        Quote Lastquote = await _context.Quotes.Where(q => q.QuoteName.Substring(0,12) == quotename).OrderBy(q => q.QuoteId).LastOrDefaultAsync();//Ultima cotizacion

                        if (Lastquote != null)
                        {
                            Lastnumber = Lastquote.QuoteName.Substring(13, 3);
                            Consec = Int32.Parse(Lastnumber) + 1;
                        }
                        else
                        {
                            Lastnumber = "001";
                            Consec = Int32.Parse(Lastnumber);
                        }

                        Lastnumber = Consec.ToString();

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
                            Currency = await _context.Currencies.FindAsync(model.CurrencyId),
                            Customer = customer,
                            CustomerPO = model.CustomerPO,
                            FinalUserId = model.FinalUserId,
                            ModifiedBy = null,
                            ModifyDate = null,
                            QuoteDate = model.QuoteDate,
                            QuoteName = quotename,
                            Seller = model.SellerId,
                            Tax = model.Tax,
                            ExchangeRate = model.ExchangeRate,
                            validUntilDate = model.validUntilDate,
                            QuoteStatus = quotestatus,
                            Subtotal = 0

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

                List<SelectListItem> currencies = (List<SelectListItem>)await _comboHelper.GetComboCurrenciesAsync(1);
                model.Currency = currencies;
                model.CurrencyId = 1;
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddQuote", model) });
            }

        }

        public async Task<IActionResult> EditQuote(int id)
        {
   

            Quote quote = await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.QuoteStatus)
                .Include(q => q.Currency)
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
            List<SelectListItem> currencies = (List<SelectListItem>)await _comboHelper.GetComboCurrenciesAsync();

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
                SellerId = quote.Seller,
                Tax = quote.Tax,
                Discount = quote.Discount,
                Active = quote.Active,
                ModifiedBy = (List<SelectListItem>)await _userHelper.GetAllUsersAsync(),
                ModifiedById = quote.ModifiedBy,
                QuoteStatus = quotestatus,
                QuoteStatusId = quote.QuoteStatus.QuoteStatusId,
                ModifyDate = quote.ModifyDate,
                Comments = quote.Comments,
                CurrencyId = quote.Currency.CurrencyId,
                Currency = currencies,
                ExchangeRate = quote.ExchangeRate

            };

            return View(editquote);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuote(EditQuote model)
        {
            Quote quote = await _context.Quotes
                .Include(q => q.QuoteDetails)
                .Include(q => q.Customer)
                .Include(q => q.QuoteStatus)
                .Include(q => q.Currency)
                .FirstOrDefaultAsync(q => q.QuoteId == model.QuoteId);

            bool validToChangeWin = true; //bandera que indica que tiene partidas y puede recibir el cambio de estatus a Ganada

            if (quote.QuoteDetails.Count == 0 && model.QuoteStatusId == 5)//Esta condición es la única que puede detener el cambio de estatus
            {
                validToChangeWin = false;
            }

            if (ModelState.IsValid && validToChangeWin)
            {
               
                try
                {
                        Customer customer = await _context.Customers.FindAsync(model.CustomerId);
                       
                        QuoteStatus quotestatus = _context.QuoteStatus.Find(model.QuoteStatusId);//Estatus 2 es en modificación

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
                        quote.Discount = model.Discount;
                        quote.validUntilDate = model.validUntilDate;
                        quote.Currency = await _context.Currencies.FindAsync(model.CurrencyId);
                        quote.CreatedBy = await _userHelper.GetUserAsync(model.CreatedBy);
                        quote.QuoteStatus = quotestatus;
                        quote.ExchangeRate = model.ExchangeRate;
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
                List<SelectListItem> currencies = (List<SelectListItem>)await _comboHelper.GetComboCurrenciesAsync();

                quotestatus = _context.QuoteStatus
                    .Select(q => new SelectListItem
                    {
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
                    Comments = quote.Comments,
                    CurrencyId = quote.Currency.CurrencyId,
                    Currency = currencies

                };

                if (validToChangeWin == false)
                {
                    ModelState.AddModelError(string.Empty, "La cotización no tiene partidas para poderla dar por ganada");
                }
              

                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditQuote", editquote) });
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
                    return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEditCommentMessage", model) });

                }

            }
            else
            {
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEditCommentMessage", model) });
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

        public ActionResult AddOrEditDetailComment(int id)
        {
            List<QuoteDetailCommentView> comments = _context.QuoteDetailComments
                .Include(c => c.User)
                .Where(c => c.QuoteDetail.QuoteDetailId == id)
                .Select(c => new QuoteDetailCommentView
                {
                    UserName = c.User.Email,
                    CommentId = c.CommentId,
                    QuoteDetailId = c.QuoteDetail.QuoteDetailId,
                    Comment = c.Comment,
                    Usuario = c.User.FullName,
                    DateComment = c.DateComment
                })
                .OrderBy(c => c.QuoteDetailId)
                .ToList();

            ViewBag.QuoteDetailId = id;
            var name = User.Identity.Name;
            User usr = _context.Users.Where(u => u.UserName == name).FirstOrDefault();
            ViewBag.UserName = name;
            ViewBag.Usuario = usr.FullName;
    
            return View(comments);


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEditDetailComment( QuoteDetailCommentView model)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    QuoteDetail qd = await _context.QuoteDetails
                        .Include(q => q.Quote)
                        .FirstOrDefaultAsync(q => q.QuoteDetailId == model.QuoteDetailId);

                    QuoteDetailComment comment = new QuoteDetailComment()
                    {
                        QuoteDetail = await _context.QuoteDetails.FindAsync(model.QuoteDetailId),
                        Comment = model.Comment,
                        DateComment = DateTime.Now,
                        User = await _userHelper.GetUserAsync(User.Identity.Name)
                    };

                    _context.Add(comment);
                    await _context.SaveChangesAsync();
                    TempData["AddOrEditDetailCommentResult"] = "true";
                    TempData["AddOrEditDetailCommentMessage"] = "El comentario fué agregado";

                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuoteDetails", _context.QuoteDetails.Where(q => q.Quote.QuoteId == qd.Quote.QuoteId).ToList()) });

                }
                catch (DbUpdateException dbUpdateException)
                {

                    ModelState.AddModelError(string.Empty, dbUpdateException.Message);
                    return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEditDetailCommentMessage", model) });

                }

            }
            else
            {
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEditDetailCommentMessage", model) });
            }

        }


        [HttpPost]
        public async Task<IActionResult> DeleteDetailComment(int id)
        {
            QuoteDetailComment comment = await _context.QuoteDetailComments.FindAsync(id);
            try
            {
                _context.QuoteDetailComments.Remove(comment);
                await _context.SaveChangesAsync();
                TempData["DeleteDetailCommentResult"] = "true";
                TempData["DeleteDetailCommentMessage"] = "El comentario fué eliminado";
                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuoteDetails", _context.Quotes.ToList()) });


            }
            catch (Exception e)
            {
                TempData["DeleteDetailCommentResult"] = "false";
                TempData["EeleteDetailCommentMessage"] = "El comentario no pudo ser eliminado error: " + e.Message;
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuoteDetails", _context.Quotes.ToList()) });

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


            //Buscamos la cotización para pasar todos sus valores al Detalle

            QuoteDetail qd = new QuoteDetail();

            qd.Quote = await _context.Quotes.FindAsync(id); 
                
               
            //El Detalle siempre traerá un id de cotización ya que sea nuevo el detalle o actualización la Quote ya existe
            QuoteDetailViewModel qdv = new QuoteDetailViewModel();
         

            //Pasamos el listado de unidades para que aparezca en un combo despues asignaremos un default si el 
            //movimiento es un Edit si es un nuevo detalle entonces se quedeará como esta sin asignar el id de la unidad
        

            if (detailid == 0)
            {
                List<SelectListItem> Unit = (List<SelectListItem>)await _comboHelper.GetComboUnitAsync();
                List<SelectListItem> Category = (List<SelectListItem>)await _comboHelper.GetComboCategoriesAsync();
                List<SelectListItem> MaterialType = (List<SelectListItem>)await _comboHelper.GetComboMaterialTypesAsync(0);
                List<SelectListItem> Material = (List<SelectListItem>)await _comboHelper.GetComboMaterialsAsync(0);
                qdv = new()
                {
                    QuoteId = qd.Quote.QuoteId,
                    Category = Category,
                    MaterialType = MaterialType,
                    Unit = Unit,

                };
                return View(qdv);
            }
            else
            {
                qd =  await _context.QuoteDetails
                           .Include(q => q.Quote)
                           .Include(q => q.Material).ThenInclude(m => m.MaterialType).ThenInclude(m => m.Category)
                           .Include(q => q.Unit)
                           .FirstOrDefaultAsync(q => q.QuoteDetailId == detailid);

                List<SelectListItem> Unit = (List<SelectListItem>)await _comboHelper.GetComboUnitAsync();
                List<SelectListItem> Category = (List<SelectListItem>)await _comboHelper.GetComboCategoriesAsync();
                List<SelectListItem> MaterialType = (List<SelectListItem>)await _comboHelper.GetComboMaterialTypesAsync(qd.Material.MaterialType.Category.CategoryId);
                List<SelectListItem> Material = (List<SelectListItem>)await _comboHelper.GetComboMaterialsAsync(qd.Material.MaterialType.MaterialTypeId);
   
                    qdv = new()
                    {
                        QuoteId = qd.Quote.QuoteId,
                        QuoteDetailId = qd.QuoteDetailId,
                        Category = Category,
                        CategoryId = qd.Material.MaterialType.Category.CategoryId,
                        MaterialTypeId = qd.Material.MaterialType.MaterialTypeId,
                        MaterialType = MaterialType,
                        MaterialId = qd.Material.MaterialId,
                        Material = Material,
                        Quantity = qd.Quantity,
                        Price = qd.Price,
                        Unit = Unit,
                        UnitId = qd.Unit.UnitId,
                        Description = qd.Description
                    };


            };
               
            return View(qdv);
            }
        

        [HttpPost]
        public async Task<IActionResult> AddOrEditDetail(QuoteDetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.QuoteDetailId == 0) {
                    try
                    {
                        QuoteDetail qd = new QuoteDetail();
                        qd.Quote = await _context.Quotes.FindAsync(model.QuoteId); ;
                        qd.Description = model.Description;
                        qd.Material = await _context.Materials.FindAsync(model.MaterialId);
                        qd.Price = model.Price;
                        qd.Quantity = model.Quantity;
                        qd.Unit = await _context.Units.FindAsync(model.UnitId);
  
                        _context.Add(qd);

                        Quote q = await _context.Quotes.FindAsync(qd.Quote.QuoteId);
                        q.QuoteStatus = await _context.QuoteStatus.FindAsync(2);
                        _context.Update(q);

                        await _context.SaveChangesAsync();

                        //Actualizamos el subtotal
                        decimal subtotal = 0;
                        List<QuoteDetail> lstquotedetails = _context.QuoteDetails.Where(d => d.Quote == q).ToList();
                        foreach (QuoteDetail d in lstquotedetails)
                        {
                            subtotal += d.Quantity * d.Price;
                        }
                        q.Subtotal = subtotal;
                        _context.Update(q);
                        await _context.SaveChangesAsync();


                        TempData["AddOrEditQuoteDetailResult"] = "true";
                        TempData["AddOrEditQuoteDetailMessage"] = "La partida fué creada";
                        return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuoteDetails", _context.QuoteDetails.Where(q => q.Quote.QuoteId == model.QuoteId).ToList() )});

                    }
                    catch (Exception)
                    {

                        return View(model);

                    }
                }
                else
                {
                    try
                    {
                        QuoteDetail qd = await _context.QuoteDetails.FindAsync(model.QuoteDetailId);

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

        [HttpPost]
        public async Task<IActionResult> CopyDetail(int quoteDetailId)
        {
         
                try
                {
                    QuoteDetail qd = await _context.QuoteDetails
                    .Include(q => q.Quote)
                    .Include(q => q.Unit)
                    .Include(q => q.Material)
                    .FirstOrDefaultAsync( q => q.QuoteDetailId == quoteDetailId);

                QuoteDetail qdn = new QuoteDetail();
                qdn.Quote = qd.Quote;
                qdn.Description = qd.Description;
                qdn.Quantity = qd.Quantity;
                qdn.Material = qd.Material;
                qdn.Price = qd.Price;               
                qdn.QuoteDetailComments = qd.QuoteDetailComments;
                qdn.Unit = qd.Unit;

                _context.Add(qdn);
                await _context.SaveChangesAsync();

                return Json(new {data = "success", isValid = true });
                }
                catch (Exception)
                {

                    return View();

                }
 


           

        }

        public JsonResult GetMaterialTypes(int categoryId)
        {
            Data.Entities.Category category = _context.Categories
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



        public async Task< FileResult> PrintReport(int QuoteId) {

            Quote quote = await _context.Quotes.FindAsync(QuoteId);

            Stream stream = new MemoryStream(await _reportHelper.GenerateQuoteReportPDFAsync(QuoteId));
            
            return File(stream, "application/pdf", $"{quote.QuoteName}{".pdf"}");
        }


        [HttpGet]
        public async Task<IActionResult> DeleteQuoteDetail(int id)
        {
            QuoteDetail qd = await _context.QuoteDetails
               .Include(d => d.Quote)
               .FirstOrDefaultAsync(d => d.QuoteDetailId == id);
            int quoteId = qd.Quote.QuoteId;


            return View(qd);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuoteDetail(QuoteDetail model)
        {

         
            try
            {
                //Borrar archivos

                int quoteid = model.Quote.QuoteId;


                List<QuoteDetailComment> lqdc = new List<QuoteDetailComment>(); 

                List<Archive> archives = _context.Archives.Where(a => a.Entity == "QuoteDetail" && a.EntityId == model.QuoteDetailId).ToList();

                foreach (var item in archives)
                {
                    try
                    {
                        await _blobHelper.DeleteBlobAsync(item.ArchiveGuid, "archives");
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                  
                    _context.Archives.Remove(item);
                }

                lqdc = _context.QuoteDetailComments.Where(c => c.QuoteDetail.QuoteDetailId == model.QuoteDetailId).ToList();
                
                foreach (var comment in lqdc)
                {
                    _context.Remove(comment);
                }
                await _context.SaveChangesAsync();

                _context.QuoteDetails.Remove(model);
                await _context.SaveChangesAsync();

                ///Recalculamos el subtotal a nivel Cotización
                ///
                Quote q = await _context.Quotes.FindAsync(quoteid);
                decimal subtotal = 0;
                List<QuoteDetail> lstquotedetails = _context.QuoteDetails.Where(d => d.Quote == q).ToList();
                foreach (QuoteDetail d in lstquotedetails)
                {
                    subtotal += d.Quantity * d.Price;
                }
                q.Subtotal = subtotal;
                _context.Update(q);
                await _context.SaveChangesAsync();

                TempData["DeleteQuoteDetailtResult"] = "true";
                TempData["DeleteQuoteDetailMessage"] = "La partida fue eliminada";
            }

            catch
            {
                TempData["DeleteQuoteDetailtResult"] = "false";
                TempData["DeleteQuoteDetailMessage"] = "La partida no pudo ser eliminada";
                return RedirectToAction(nameof(Details), new { id = model.Quote.QuoteId });

            }

            return RedirectToAction(nameof(Details), new { id = model.Quote.QuoteId });

        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, int statusid)
        {
            Quote quote = await _context.Quotes
                 .Include(q => q.Customer)
                 .Include(q => q.QuoteStatus)
                 .FirstOrDefaultAsync(m => m.QuoteId == id);
            try
            {

                quote.QuoteStatus = await  _context.QuoteStatus.FindAsync(statusid);
                _context.Update(quote);
                await _context.SaveChangesAsync();  

                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuoteDetails", _context.QuoteDetails.Where(q => q.Quote.QuoteId == quote.QuoteId).ToList()) });

            }
            catch (DbUpdateException dbUpdateException)
            {

                ModelState.AddModelError(string.Empty, dbUpdateException.Message);

                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuoteDetails", _context.QuoteDetails.Where(q => q.Quote.QuoteId == quote.QuoteId).ToList()) });

            }

        }

        [HttpPost]
        public async Task<IActionResult> ChangeDiscount(int id, decimal discount)
        {
            Quote quote = await _context.Quotes
                 .Include(q => q.Customer)
                 .Include(q => q.QuoteStatus)
                 .FirstOrDefaultAsync(m => m.QuoteId == id);
            try
            {

                quote.Discount = discount;
                _context.Update(quote);
                await _context.SaveChangesAsync();
                

                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuoteDetails", _context.QuoteDetails.Where(q => q.Quote.QuoteId == quote.QuoteId).ToList()) });

            }
            catch (DbUpdateException dbUpdateException)
            {

                ModelState.AddModelError(string.Empty, dbUpdateException.Message);

                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuoteDetails", _context.QuoteDetails.Where(q => q.Quote.QuoteId == quote.QuoteId).ToList()) });

            }

        }

        [HttpPost]
        public async Task<IActionResult> ChangeComments(int id, string comments)
        {
            Quote quote = await _context.Quotes.FirstOrDefaultAsync(m => m.QuoteId == id);
            try
            {

                quote.Comments = comments;
                _context.Update(quote);
                await _context.SaveChangesAsync();

                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuoteDetails", _context.QuoteDetails.Where(q => q.Quote.QuoteId == quote.QuoteId).ToList()) });

            }
            catch (DbUpdateException dbUpdateException)
            {

                ModelState.AddModelError(string.Empty, dbUpdateException.Message);

                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllQuoteDetails", _context.QuoteDetails.Where(q => q.Quote.QuoteId == quote.QuoteId).ToList()) });

            }

        }


        [HttpGet]
        public async Task<IActionResult> CopyQuote(int id)
        {
            Quote quote = await _context.Quotes.FirstOrDefaultAsync(q => q.QuoteId == id);
            CopyQuote cquote = new CopyQuote();
            cquote.QuoteId = quote.QuoteId;
            cquote.QuoteName = quote.QuoteName;
            cquote.copyfilesdetails = false;
            cquote.copyfilesheader = false;
            return View(cquote);
        }

        [HttpPost]
        public async Task<IActionResult> CopyQuote(CopyQuote model)
        {

            ////cotización de la que se basa la nueva
            ///
            Quote oldquote = await _context.Quotes
                .Include(q => q.QuoteDetails).ThenInclude(q => q.Material)
                .Include(q => q.QuoteDetails).ThenInclude(q => q.Unit)
                .Include(q => q.Customer)
                .Include(q => q.Currency)
                .FirstOrDefaultAsync(q => q.QuoteId == model.QuoteId);

       

            //Formamos el nombre
            String quotename = DateTime.Now.ToString("yyyyMMdd"); //Variable formadora de nombre de coti
            String Lastnumber = ""; //Ultimo numero consecutivo de la cotización
            String strnumber = "";
            int Consec = 0;

            ///Por default dejamos 10 dias de vigencia
            TimeSpan validuntildate = new TimeSpan(10, 0, 0, 0); //Diez dias de vigencia por defecto


            quotename = "COT-" + quotename;

            // -------------------------

            Quote Lastquote = await _context.Quotes.Where(q => q.QuoteName.Substring(0, 12) == quotename.Substring(0, 12)).OrderBy(q => q.QuoteId).LastOrDefaultAsync();//Ultima cotizacion

            if (Lastquote != null)
            {
                Lastnumber = Lastquote.QuoteName.Substring(13, 3);
                Consec = Int32.Parse(Lastnumber);
            }
            else
            {
                Lastnumber = "000";
                Consec = Int32.Parse(Lastnumber);
            }

            Consec +=  1;

            strnumber = $"000{Consec}";

            quotename = quotename + "-" + strnumber.Substring(strnumber.Length - 3, 3);

            //-----------------------

            User createdBy = await _userHelper.GetUserAsync(User.Identity.Name);
            QuoteStatus quotestatus = await _context.QuoteStatus.FindAsync(2);//Como es copia nace en modificación

            Quote quote = new Quote()
            {
                Active = true,
                BuyerContactId = oldquote.BuyerContactId,
                Comments = oldquote.Comments,
                CreatedBy = createdBy,
                Currency = oldquote.Currency,
                Customer = oldquote.Customer,
                CustomerPO = oldquote.CustomerPO,
                FinalUserId = oldquote.FinalUserId,
                ModifiedBy = null,
                ModifyDate = null,
                QuoteDate = DateTime.Now,
                QuoteName = quotename,
                Seller = oldquote.Seller,
                Tax = oldquote.Tax,
                validUntilDate = DateTime.Now.Add(validuntildate),
                QuoteStatus = quotestatus,

            };
            _context.Add(quote);
            await _context.SaveChangesAsync();

            foreach (QuoteDetail oqd in oldquote.QuoteDetails) {
                QuoteDetail qd = new QuoteDetail();
                qd.Quote = quote;
                qd.Description = oqd.Description;
                qd.Material = oqd.Material;
                qd.Price = oqd.Price;
                qd.Quantity = oqd.Quantity;
                qd.Unit = oqd.Unit;
                _context.Add(qd);
                await _context.SaveChangesAsync();
                //Agregaremos todos los archivos ligados a cada detalles si fue palomeado el checkbox de model.copyfilesdetails

                if (model.copyfilesdetails == true)
                {
                    List<Archive> archives = _context.Archives.Where(a => a.Entity == "QuoteDetail" && a.EntityId == oqd.QuoteDetailId).ToList();

                    foreach (Archive arch in archives)
                    {
                        
                        Guid archiveguid = Guid.Empty;

                        archiveguid = await _blobHelper.CopyBlobAsync(arch.ArchiveGuid, "archives");

                        Archive archive = new Archive();
                        archive.ArchiveGuid = archiveguid;
                        archive.Entity = arch.Entity;
                        archive.EntityId = qd.QuoteDetailId;
                        archive.ArchiveName = arch.ArchiveName;
                        _context.Add(archive);
                        await _context.SaveChangesAsync();
                    }
                   
                }
               
            }
            

            //Si se eligio copiar archivos de cabecera los copiamos a la nueva Cotizacion

            if (model.copyfilesheader == true)
            {
                List<Archive> qarchives = _context.Archives.Where(a => a.Entity == "Quote" && a.EntityId == oldquote.QuoteId).ToList();

                foreach (Archive a in qarchives)
                {
                    Guid archiveguid = Guid.Empty;
                    archiveguid = await _blobHelper.CopyBlobAsync(a.ArchiveGuid, "archives");
                    Archive archive = new Archive();
                    archive.ArchiveGuid = archiveguid;
                    archive.Entity = a.Entity;
                    archive.EntityId = quote.QuoteId;
                    archive.ArchiveName = a.ArchiveName;
                    _context.Add(archive);
                    await _context.SaveChangesAsync();

                }
                
            }

            TempData["CopyQuoteResult"] = "true";
            TempData["CopyQuoteMessage"] = "La Cotización fué copiada";
            return RedirectToAction("Index", "Quotes", new { id = quote.QuoteId });
        }

        [HttpGet]
        public async Task<IActionResult> CreateWorkOrder(int id)
        {
            Quote quote = await _context.Quotes
               .FindAsync(id);
            return View(quote);
        }


        [HttpPost]
        public async Task<IActionResult> CreateWorkOrder(Quote model)
        {
            Quote quote = await _context.Quotes
               .Include(q => q.QuoteDetails).ThenInclude(q => q.Material)
               .Include(q => q.QuoteDetails).ThenInclude(q => q.Unit)
               .Include(q => q.QuoteDetails).ThenInclude(q => q.Unit)
               .Include(q => q.Currency)
               .Include(q => q.Customer)
               .FirstOrDefaultAsync(q => q.QuoteId == model.QuoteId);



            String workordername = DateTime.Now.ToString("yyyyMMdd"); //Variable formadora de nombre de coti
            String Lastnumber = ""; //Ultimo numero consecutivo de la cotización
            String strnumber = "";
            int Consec = 0;


            workordername = "OT-" + workordername;

            // -------------------------

            WorkOrder LastWO = await _context.WorkOrders.Where(q => q.WorkOrderName.Substring(0, 1) == workordername).OrderBy(w => w.WorkOrderId).LastOrDefaultAsync();//Ultima cotizacion

            if (LastWO != null)
            {
                Lastnumber = LastWO.WorkOrderName.Substring(7, 3);
                Consec = Int32.Parse(Lastnumber);
            }
            else
            {
                Lastnumber = "000";
                Consec = Int32.Parse(Lastnumber);
            }

            Lastnumber += 1;

            strnumber = $"000{Lastnumber}";

            workordername = workordername + "-" + strnumber.Substring(strnumber.Length - 3, 3);


            ///--------------------------

            WorkOrder workOrder = new WorkOrder();
            workOrder.Active = true;
            workOrder.BuyerContactId = quote.BuyerContactId;
            workOrder.Comments = quote.Comments;
            workOrder.CreatedBy = await _userHelper.GetUserAsync(User.Identity.Name);
            workOrder.Currency = quote.Currency;
            workOrder.ExchangeRate = quote.ExchangeRate;
            workOrder.Customer = quote.Customer;
            workOrder.CustomerPO = quote.CustomerPO;
            workOrder.QuoteId = quote.QuoteId;
            workOrder.FinalUserId = quote.FinalUserId;
            workOrder.Seller = quote.Seller;
            workOrder.Tax = quote.Tax;
            workOrder.WorkOrderDate = DateTime.Now;
            workOrder.WorkOrderName = workordername;
            workOrder.ExchangeRate = quote.ExchangeRate;
            workOrder.WorkOrderStatus = await _context.WorkOrderStatus.FindAsync(1);
            
            _context.Add(workOrder);
            await _context.SaveChangesAsync();

            foreach (QuoteDetail qd in quote.QuoteDetails) 
            { 
                WorkOrderDetail wod = new WorkOrderDetail();
                wod.Description = qd.Description;
                wod.Material = qd.Material;
                wod.Price = qd.Price;
                wod.Quantity = qd.Quantity; 
                wod.Unit = qd.Unit;
                wod.WorkOrder = workOrder;
                _context.Add(wod);
                await _context.SaveChangesAsync();  

            };

            quote.QuoteStatus = await _context.QuoteStatus.FindAsync(7);
            await _context.SaveChangesAsync();

            TempData["CreateWorkOrderResult"] = "true";
            TempData["CreateWorkOrderMessage"] = $"La Orden de trabajo {workordername} fué creada";

            return RedirectToAction("Index", "Quotes");
        }
        private bool QuoteExists(int id)
        {
          return _context.Quotes.Any(e => e.QuoteId == id);
        }
    }
}
