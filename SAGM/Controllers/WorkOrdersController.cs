using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SAGM.Data;
using SAGM.Data.Entities;
using SAGM.Helpers;
using System.Data;
using ClosedXML.Excel;
using SAGM.Models;
using Azure.Storage.Blobs;
using static SAGM.Helpers.ModalHelper;
using System.Diagnostics.Contracts;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace SAGM.Controllers
{
    public class WorkOrdersController : Controller
    {
        private readonly SAGMContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IComboHelper _comboHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IConfiguration _configuration;
        private readonly IReportHelper _reportHelper;

        public WorkOrdersController(SAGMContext context, IUserHelper userHelper, IComboHelper comboHelper, IBlobHelper blobHelper, IConfiguration configuration, IReportHelper reportHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _comboHelper = comboHelper;
            _blobHelper = blobHelper;
            _configuration = configuration;
            _reportHelper = reportHelper;
        }

        public IActionResult Index()
        {

            ViewBag.Result = "";
            ViewBag.Message = "";


            if (TempData["AddWorkOrderResult"] != null)
            {

                ViewBag.Result = TempData["AddWorkOrderResult"].ToString();
                ViewBag.Message = TempData["AddWorkOrderMessage"].ToString();
                TempData.Remove("AddWorkOrderResult");
                TempData.Remove("AddWorkOrderMessage");
            };

            if (TempData["EditWorkOrderResult"] != null)
            {

                ViewBag.Result = TempData["EditWorkOrderResult"].ToString();
                ViewBag.Message = TempData["EditWorkOrderMessage"].ToString();
                TempData.Remove("EditWorkOrdeResult");
                TempData.Remove("EditWorkOrdeMessage");
            };


            
            if (TempData["AddOrEditCommentResult"] != null)
            {
                ViewBag.Result = TempData["AddOrEditCommentResult"].ToString();
                ViewBag.Message = TempData["AddOrEditCommentMessage"].ToString();
                TempData.Remove("AddOrEditCommentResult");
                TempData.Remove("AddOrEditCommentMessage");
            };

         
            if (TempData["DeleteWOCommentResult"] != null)
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

            return View();

        }

        public async Task<JsonResult> GetWorkOrders()
        {
            List<WorkOrder> workorders = await _context.WorkOrders
                 .Include(w => w.WorkOrderDetails)
                 .ThenInclude(d => d.Material)
                 .Include(w => w.WorkOrderDetails)
                 .ThenInclude(d => d.Unit)
                 .Include(w => w.Customer)
                 .Include(w => w.WorkOrderStatus)
                 .Include(w => w.Currency)
                 .OrderByDescending(w => w.WorkOrderId)
                 .ToListAsync();

            List<WorkOrderViewIndexModel> lworkOrders = new List<WorkOrderViewIndexModel>();

            foreach (WorkOrder w in workorders)
            {
                string seller = _userHelper.GetUserAsync(w.Seller).Result.FullName.ToString();
                Contact buyercontact = await _context.Contacts.FindAsync(w.BuyerContactId);
                Contact finaluser = await _context.Contacts.FindAsync(w.FinalUserId);
                int archivesnumber = 0;
                Quote quote = _context.Quotes.FirstOrDefault(q => q.QuoteId == w.QuoteId);

             

                //Armamos la lista de detalles
                List<WorkOrderDetailViewIndexModel> details = new List<WorkOrderDetailViewIndexModel>();
                foreach (WorkOrderDetail wd in w.WorkOrderDetails)
                {
                    List<Archive> archives = _context.Archives.Where(a => a.Entity == "WorkOrderDetail" && a.EntityId == wd.WorkOrderDetailId).ToList();
                    archivesnumber += archives.Count;
                    WorkOrderDetailViewIndexModel wdvim = new WorkOrderDetailViewIndexModel()
                    {
                        Quantity = wd.Quantity,
                        Material = _context.Materials.FindAsync(wd.Material.MaterialId).Result.MaterialName.ToString(),
                        Description = wd.Description,
                        Price = wd.Price,
                        RawMaterial = wd.RawMaterial,   
                        Machined = wd.Machined,
                        TT = wd.TT,
                        Shipped = wd.Shipped,
                        Invoiced = wd.Invoiced
                    };
                    details.Add(wdvim);

                }

                //

                List<Archive> warchives = _context.Archives.Where(a => a.Entity == "WorkOrder" && a.EntityId == w.WorkOrderId).ToList();

                string archiveschain = "";

                foreach (var item in warchives)
                {
                    archiveschain += item.ArchiveGuid.ToString() + "," + item.ArchiveName + "," + item.ArchiveId + "|";
                }
                if (archiveschain != "")
                {
                    archiveschain = archiveschain.Substring(0, archiveschain.Length - 1);
                };

                
                WorkOrderViewIndexModel aqs = new WorkOrderViewIndexModel()
                {
                    WorkOrderId = w.WorkOrderId,
                    WorkOrderDate = w.WorkOrderDate,
                    Active = w.Active,
                    BuyerContact = $"{buyercontact.Name} {buyercontact.LastName}",
                    Comments = w.Comments,
                    CreatedBy = w.CreatedBy.FullName,
                    Currency = w.Currency.Curr,
                    ExchangeRate = w.ExchangeRate,
                    CustomerNickName = w.Customer.CustomerNickName,
                    CustomerPO = w.CustomerPO,
                    FinalUser = $"{finaluser.Name} {finaluser.LastName}",
                    ModifiedBy = w.ModifiedBy,
                    ModifyDate = w.ModifyDate,
                    WorkOrderDetails = details,
                    WorkOrderName = w.WorkOrderName,
                    Seller = seller,
                    Tax = w.Tax,
                    WorkOrderStatusName = w.WorkOrderStatus.WorkOrderStatusName,
                    PromiseDate = w.PromiseDate,
                    ArchivesNumber = archivesnumber,
                    ArchivesChain = archiveschain,
             
                };

                if (quote != null)
                {
                    aqs.QuoteId = quote.QuoteId;
                    aqs.QuoteName = quote.QuoteName;
                }
                else
                {
                    aqs.QuoteId = 0;
                    aqs.QuoteName = "";
                }

                lworkOrders.Add(aqs);
            }

            return Json(new { data = lworkOrders.OrderByDescending(o => o.WorkOrderId) });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id, int? workOrderDetilId)
        {


            ViewBag.Result = "";
            ViewBag.Message = "";
            ViewBag.workOrderDetilId = workOrderDetilId.ToString();

            if (TempData != null)
            {
                if (TempData["AddOrEditWorkOrderDetailResult"] != null)
                {

                    ViewBag.Result = TempData["AddOrEditWorkOrderDetailResult"].ToString();
                    ViewBag.Message = TempData["AddOrEditWorkOrderDetailMessage"].ToString();
                    ViewBag.quoteDetailId = workOrderDetilId.ToString();
                    TempData.Remove("AddOrEditworkOrderDetailResult");
                    TempData.Remove("AddOrEditworkOrderDetailMessage");
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
                if (TempData["DeleteWorkOrderDetailResult"] != null)
                {
                    ViewBag.Result = TempData["DeleteWorkOrderDetailResult"].ToString();
                    ViewBag.Message = TempData["DeleteWorkOrderDetailMessage"].ToString();
                    TempData.Remove("DeleteWorkOrderDetailResult");
                    TempData.Remove("DeleteWorkOrderDetailMessage");
                }
                if (TempData["AddOrEditDetailCommentResult"] != null)
                {
                    ViewBag.Result = TempData["AddOrEditDetailCommentResult"].ToString();
                    ViewBag.Message = TempData["AddOrEditDetailCommentMessage"].ToString();
                    TempData.Remove("AddOrEditDetailCommentResult");
                    TempData.Remove("AddOrEditDetailCommentMessage");
                };


                if (TempData["AddOrEditWorkOrderDetailProcessResult"] != null) 
                {
                    ViewBag.Result = TempData["AddOrEditWorkOrderDetailProcessResult"].ToString();
                    ViewBag.Message = TempData["AddOrEditWorkOrderDetailMessage"].ToString();
                    TempData.Remove("AddOrEditWorkOrderDetailProcessResult");
                    TempData.Remove("AddOrEditWorkOrderDetailMessage");
                }

                if (TempData["WorkOrderDetailProcessDeleteResult"] != null)
                {
                    ViewBag.Result = TempData["WorkOrderDetailProcessDeleteResult"].ToString();
                    ViewBag.Message = TempData["WorkOrderDetailProcessDeleteMessage"].ToString();
                    TempData.Remove("WorkOrderDetailProcessDeleteResult");
                    TempData.Remove("WorkOrderDetailProcessDeleteMessage");
                }

            }


            if (id == null || _context.WorkOrders == null)
            {
                return NotFound();
            }

            var workorder = await _context.WorkOrders
                .Include(q => q.Customer)
                .Include(q => q.WorkOrderDetails)
                .Include(q => q.WorkOrderStatus)
                .FirstOrDefaultAsync(m => m.WorkOrderId == id);

            ViewBag.DetailsCount = workorder.WorkOrderDetails.Count();

            if (workorder == null)
            {
                return NotFound();
            }

            List<SelectListItem> workorderstatus = (List<SelectListItem>)await _comboHelper.GetComboWorkOrderStatus(workorder.WorkOrderStatus.WorkOrderStatusId);


            User seller = await _userHelper.GetUserAsync(workorder.Seller);

            WorkOrderViewModel workorderv = new WorkOrderViewModel();
            Contact buyer = await _context.Contacts.FindAsync(workorder.BuyerContactId);
            Contact finaluser = await _context.Contacts.FindAsync(workorder.FinalUserId);
            workorderv.WorkOrderId = workorder.WorkOrderId;
            workorderv.Active = workorder.Active;
            workorderv.BuyerContactId = workorder.BuyerContactId;
            workorderv.BuyerName = $"{buyer.Name} {buyer.LastName}";
            workorderv.Comments = workorder.Comments;
            workorderv.CreatedBy = workorder.CreatedBy;
            workorderv.Currency = workorder.Currency;
            workorderv.ExchangeRate = workorder.ExchangeRate;
            workorderv.Customer = workorder.Customer;
            workorderv.CustomerPO = workorder.CustomerPO;
            workorderv.FinalUserId = workorder.FinalUserId;
            workorderv.FinalUserName = $"{finaluser.Name} {finaluser.LastName}";
            workorderv.ModifiedBy = workorder.ModifiedBy;
            workorderv.ModifyDate = workorder.ModifyDate;
            workorderv.WorkOrderComments = workorder.WorkOrderComments;
            workorderv.Seller = workorder.Seller;
            workorderv.SellerName = seller.FullName;
            workorderv.WorkOrderName = workorder.WorkOrderName;
            workorderv.WorkOrderstatus = workorderstatus;
            workorderv.WorkOrderStatusId = workorder.WorkOrderStatus.WorkOrderStatusId;
            workorderv.Tax = workorder.Tax;


            return View(workorderv);

        }

        [HttpGet]
        public async Task<JsonResult> GetWorkOrderDetails(int id)
        {

            var workorder = await _context.WorkOrders
                .Include(w => w.WorkOrderDetails).ThenInclude(w => w.Unit)
                .Include(w => w.WorkOrderDetails).ThenInclude(w => w.Material)
                .Include(w => w.WorkOrderDetails).ThenInclude(w => w.WorkOrderDetailProcess)
                .FirstOrDefaultAsync(w => w.WorkOrderId == id);
            if (workorder == null)
            {
                return Json(new { });
            }

            User seller = await _userHelper.GetUserAsync(workorder.Seller);

           WorkOrderViewModel workorderv = new WorkOrderViewModel();


            List<AllWorkOrderDetails> details = new List<AllWorkOrderDetails>().ToList();
            if (workorder.WorkOrderDetails != null)
            {

                foreach (var detail in workorder.WorkOrderDetails)
                {

                    List<Archive> archives = _context.Archives.Where(a => a.Entity == "WorkOrderDetail" && a.EntityId == detail.WorkOrderDetailId).ToList();

                    string archiveschain = "";

                    foreach (var item in archives)
                    {
                        archiveschain += item.ArchiveGuid.ToString() + "," + item.ArchiveName + "," + item.ArchiveId + "|";
                    }
                    if (archiveschain != "")
                    {
                        archiveschain = archiveschain.Substring(0, archiveschain.Length - 1);
                    };

                    List<WorkOrderDetailProcess> wodp = _context.WorkOrderDetailProcesses
                        .Include(w => w.Machine).ThenInclude(w => w.Process)
                        .Include(w => w.Unit)
                        .Include(w => w.WorkOrderDetail)
                        .Where(a => a.WorkOrderDetail.WorkOrderDetailId == detail.WorkOrderDetailId).ToList();

                    string processchain = "";

                    foreach (var item in wodp)
                    {
                        processchain += item.Machine.Process.ProcessName + "," + item.Machine.MachineName + "," + item.Unit.UnitName + "," + item.Quantity + "," + item.WorkOrderDetailProcessId.ToString() + "|";
                    }
                    if (processchain != "")
                    {
                        processchain = processchain.Substring(0, processchain.Length - 1);
                    };


                        AllWorkOrderDetails detailsv = new()
                    {
                        WorkOrderDetailId = detail.WorkOrderDetailId,
                        WorkOrder = detail.WorkOrder,
                        Description = detail.Description,
                        Material = detail.Material,
                        MaterialName = detail.Material.MaterialName,
                        Price = detail.Price,
                        Quantity = detail.Quantity,
                        Unit = detail.Unit,
                        UnitName = detail.Unit.UnitName,
                        Archives = archives,
                        ArchivesChain = archiveschain,
                        RawMaterial = detail.RawMaterial,
                        TT = detail.TT,
                        Machined = detail.Machined,
                        Invoiced = detail.Invoiced,
                        Shipped = detail.Shipped,
                        Processchain = processchain,

                        };


                    details.Add(detailsv);
                }

            }


            return Json(new { data = details });
        }

        public IActionResult WorkLoad() 
        {
            List<WorkLoad> lstworkload = new List<WorkLoad>();
            return View(lstworkload); 
        }

        [HttpGet]

        public async Task<JsonResult> GetWorkLoad()
        { 

            List<WorkLoad> lstworkload = new List<WorkLoad>();
            

            List<WorkOrder> workOrders = await _context.WorkOrders
                .Include(w => w.Customer)
                .Include(w => w.WorkOrderStatus)
                .Include(w => w.Currency)
                .Include(w => w.WorkOrderDetails).ThenInclude(d => d.Material)
                .Include(w => w.WorkOrderDetails).ThenInclude(d => d.Unit)
                .Include(w => w.WorkOrderDetails).ThenInclude(d => d.WorkOrderDetailProcess).ThenInclude(p => p.Machine).ThenInclude(m => m.Process)
                .Include(w => w.WorkOrderDetails).ThenInclude(d => d.WorkOrderDetailProcess).ThenInclude(p => p.Unit)
                .Where(w => w.WorkOrderStatus.WorkOrderStatusId == 1
                         || w.WorkOrderStatus.WorkOrderStatusId == 2
                         || w.WorkOrderStatus.WorkOrderStatusId == 3
                         || w.WorkOrderStatus.WorkOrderStatusId == 4
                         || w.WorkOrderStatus.WorkOrderStatusId == 5
                         || w.WorkOrderStatus.WorkOrderStatusId == 6).ToListAsync();

            foreach (WorkOrder wo in workOrders)
            {

               

                foreach (WorkOrderDetail wod in wo.WorkOrderDetails)
                {
                    WorkLoad workload = new WorkLoad();
                    var quotename = "";
                    var quoteid = 0;
                    if (wo.QuoteId != null)
                    {
                        quotename = _context.Quotes.FindAsync(wo.QuoteId).Result.QuoteName;
                        quoteid = wo.QuoteId.Value;
                    }
                    else
                    {
                        quotename = "";
                    }
                   


                    Contact contact = await _context.Contacts.FindAsync(wo.FinalUserId);
                    Contact buyer = await _context.Contacts.FindAsync(wo.BuyerContactId);

                    workload.WorkOrderId = wo.WorkOrderId;
                    workload.QuoteId = quoteid;
                    workload.WorkOrderName = wo.WorkOrderName;
                    workload.QuoteName = quotename;
                    workload.WorkOrderStatusName = wo.WorkOrderStatus.WorkOrderStatusName;
                    workload.CustomerNickName = wo.Customer.CustomerNickName;
                    workload.CustomerPO = wo.CustomerPO;
                    workload.FinalUser = $"{contact.Name} {contact.LastName}";
                    workload.Buyer = $"{buyer.Name} {buyer.LastName}";
                    workload.Seller = wo.Seller;
                    workload.PODate = wo.WorkOrderDate;//Este hay que corregirlo
                    workload.PromiseDate = wo.PromiseDate;
                    workload.ShippedDate = wo.ModifyDate; //Este hay que corregirlo ya que solo tendrá valor hasta que se haya entregado toda la
                    workload.Currency = wo.Currency.Curr;
                    workload.ExchangeRate = wo.ExchangeRate;
                    string process = "";
                    workload.WorkOrderDetailId = wod.WorkOrderDetailId;
                    workload.Description = wod.Description;
                    workload.Quantity = wod.Quantity;
                    workload.Price = wod.Price;
                    workload.MaterialName = wod.Material.MaterialName;
                    workload.UnitName = wod.Unit.UnitName;
                    workload.RawMaterial = wod.RawMaterial;
                    workload.Machined = wod.Machined;
                    workload.TT = wod.TT;
                    workload.Shipped = wod.Shipped;
                    workload.Invoiced = wod.Invoiced;
        

                    if (wod.WorkOrderDetailProcess != null)
                    {
                        foreach (WorkOrderDetailProcess p in wod.WorkOrderDetailProcess)
                        {
                            string machinename = "";
                            string processname = "";

                            machinename = p.Machine.MachineName;
                            processname = p.Machine.Process.ProcessName;
                            process += $"{p.Machine.MachineName}\\{p.Machine.Process.ProcessName}\\{p.Quantity}\\{p.Unit.UnitName}|";
                        }
                       
                    }
                    if (process.Length > 0)
                    {
                        process = process.Substring(0, process.Length - 1);
                    }
                    workload.ProcessArray = process;
                    lstworkload.Add(workload);
                }
            }
            return Json(new { data = lstworkload.OrderByDescending(w => w.WorkOrderDetailId) });
  

        }

        [HttpGet]
        public async Task<IActionResult> AddWorkOrder()
        {


            AddWorkOrder workorder = new AddWorkOrder();

            //-----------------------

            List<SelectListItem> sellerlist = (List<SelectListItem>)await _userHelper.GetSellersAsync();
            List<SelectListItem> users = new List<SelectListItem>();
            List<SelectListItem> sellers = new List<SelectListItem>();
            List<SelectListItem> workorderstatus = new List<SelectListItem>();

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
            List<SelectListItem> currencies = (List<SelectListItem>)await _comboHelper.GetComboCurrenciesAsync();     
            
            workorder.Customers = customers;
            workorder.Sellers = sellers;
            workorder.Currency = currencies;
            workorder.WorkOrderDate = DateTime.Now; 

            return View(workorder);
        }

        [HttpGet]
        public async Task<IActionResult> EditWorkOrder(int id)
        {


            WorkOrder workorder = await _context.WorkOrders
                .Include(w => w.Customer)
                .Include(w => w.WorkOrderStatus)
                .Include(w => w.Currency)
                .FirstOrDefaultAsync(w => w.WorkOrderId == id);

            //-----------------------

            List<SelectListItem> sellerlist = (List<SelectListItem>)await _userHelper.GetSellersAsync();
            List<SelectListItem> users = new List<SelectListItem>();
            List<SelectListItem> sellers = new List<SelectListItem>();
            List<SelectListItem> workorderstatus = new List<SelectListItem>();

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
            List<SelectListItem> customerbuyercontacts = (List<SelectListItem>)await _comboHelper.GetComboContactCustomersAsync(workorder.Customer.CustomerId);
            List<SelectListItem> customerfinalcontacts = (List<SelectListItem>)await _comboHelper.GetComboContactCustomersAsync(workorder.Customer.CustomerId);
            List<SelectListItem> currencies = (List<SelectListItem>)await _comboHelper.GetComboCurrenciesAsync();

            workorderstatus = _context.WorkOrderStatus
                .Select(w => new SelectListItem
                {
                    Text = w.WorkOrderStatusName,
                    Value = w.WorkOrderStatusId.ToString()
                }).OrderBy(c => c.Value).ToList();


            EditWorkOrder editworkorder = new EditWorkOrder()
            {
                WorkOrderId = workorder.WorkOrderId,
                WorkOrderName = workorder.WorkOrderName,
                WorkOrderDate = workorder.WorkOrderDate,
                PromiseDate = workorder.PromiseDate,
                Customers = customers,
                CustomerId = workorder.Customer.CustomerId,
                CustomerBuyerContacts = customerbuyercontacts,
                BuyerContactId = workorder.BuyerContactId,
                CustomerFinalContacts = customerfinalcontacts,
                FinalUserId = workorder.FinalUserId,
                CustomerPO = workorder.CustomerPO,
                Sellers = sellers,
                SellerId = workorder.Seller,
                Tax = workorder.Tax,
                Active = workorder.Active,
                ModifiedBy = (List<SelectListItem>)await _userHelper.GetAllUsersAsync(),
                ModifiedById = workorder.ModifiedBy,
                WorkOrderStatus = workorderstatus,
                WorkOrderStatusId = workorder.WorkOrderStatus.WorkOrderStatusId,
                ModifyDate = workorder.ModifyDate,
                Comments = workorder.Comments,
                CurrencyId = workorder.Currency.CurrencyId,
                Currency = currencies,
                ExchangeRate = workorder.ExchangeRate

            };

            return View(editworkorder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWorkOrder(AddWorkOrder model)
        {
            if (ModelState.IsValid)
            {

                try
                {
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
                    WorkOrder workorder = new WorkOrder();
                    Customer customer = await _context.Customers.FindAsync(model.CustomerId);
                    WorkOrderStatus workorderstatus = _context.WorkOrderStatus.Find(1);//Estatus 1 cread

                    workorder.WorkOrderName = workordername;
                    workorder.Active = model.Active;
                    workorder.BuyerContactId = model.BuyerContactId;
                    workorder.Comments = model.Comments;
                    workorder.Customer = customer;
                    workorder.CustomerPO = model.CustomerPO;
                    workorder.FinalUserId = model.FinalUserId;
                    workorder.ModifiedBy = User.Identity.Name;
                    workorder.ModifyDate = DateTime.Now;
                    workorder.WorkOrderDate = model.WorkOrderDate;
                    workorder.Seller = model.SellerId;
                    workorder.WorkOrderName = workordername;
                    workorder.Tax = model.Tax;
                    workorder.PromiseDate = model.PromiseDate;
                    workorder.Currency = await _context.Currencies.FindAsync(model.CurrencyId);
                    workorder.ExchangeRate = model.ExchangeRate;
                    workorder.CreatedBy = await _userHelper.GetUserAsync(model.CreatedBy);
                    workorder.WorkOrderStatus = workorderstatus;
             
                    workorder.Comments = model.Comments;
                    _context.Add(workorder);
                    await _context.SaveChangesAsync();
                    TempData["AddWorkOrderResult"] = "true";
                    
                    TempData["AddWorkOrderMessage"] = "La Orden de trabajo fué creada";
                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllWorkOrders", _context.WorkOrders.ToList()) });

                }
                catch (DbUpdateException dbUpdateException)
                {


                    ModelState.AddModelError(string.Empty, dbUpdateException.Message);
                    return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddWorkOrder", model) });

                }

            }
            else
            {

                WorkOrder workorder = new WorkOrder();
                Customer customer = await _context.Customers.FindAsync(model.CustomerId);
       

                workorder.Active = model.Active;
                workorder.BuyerContactId = model.BuyerContactId;
                workorder.Comments = model.Comments;
                workorder.Customer = customer;
                workorder.CustomerPO = model.CustomerPO;
                workorder.FinalUserId = model.FinalUserId;
                workorder.ModifiedBy = User.Identity.Name;
                workorder.ModifyDate = DateTime.Now;
                workorder.WorkOrderDate = model.WorkOrderDate;
                workorder.Seller = model.SellerId;
                workorder.WorkOrderName = model.WorkOrderName;
                workorder.Tax = model.Tax;
                workorder.PromiseDate = model.PromiseDate;
                workorder.Currency = await _context.Currencies.FindAsync(model.CurrencyId);
                workorder.ExchangeRate = model.ExchangeRate;
                workorder.CreatedBy = await _userHelper.GetUserAsync(model.CreatedBy);
                
                workorder.Comments = model.Comments;

                List<SelectListItem> sellerlist = (List<SelectListItem>)await _userHelper.GetSellersAsync();
                List<SelectListItem> users = new List<SelectListItem>();
                List<SelectListItem> sellers = new List<SelectListItem>();
                List<SelectListItem> workorderstatus = new List<SelectListItem>();
                workorderstatus = _context.WorkOrderStatus
        .Select(w => new SelectListItem
        {
            Text = w.WorkOrderStatusName,
            Value = w.WorkOrderStatusId.ToString()
        }).OrderBy(c => c.Value).ToList();


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
                List<SelectListItem> currencies = (List<SelectListItem>)await _comboHelper.GetComboCurrenciesAsync();


                AddWorkOrder addworkorder = new AddWorkOrder()
                {
                    WorkOrderDate = model.WorkOrderDate,
                    PromiseDate = model.PromiseDate,
                    Customers = customers,
                    CustomerId = model.CustomerId,
                    CustomerPO = model.CustomerPO,
                    CustomerBuyerContacts = customerbuyercontacts,
                    BuyerContactId = model.BuyerContactId,
                    CustomerFinalContacts = customerfinalcontacts,
                    FinalUserId = model.FinalUserId,
                    Sellers = sellers,
                    SellerId = model.SellerId,
                    Tax = model.Tax,
                    Active = true,
                    ModifiedBy = (List<SelectListItem>)await _userHelper.GetAllUsersAsync(),
                    ModifyDate = null,
                    Comments = workorder.Comments,
                    CurrencyId = model.CurrencyId,
                    Currency = currencies,
                    ExchangeRate = workorder.ExchangeRate,
                    WorkOrderStatus = workorderstatus
                };

                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddWorkOrder", addworkorder) });
            }

        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWorkOrder(EditWorkOrder model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Customer customer = await _context.Customers.FindAsync(model.CustomerId);
                    WorkOrder workorder = _context.WorkOrders.Find(model.WorkOrderId);
                    WorkOrderStatus workorderstatus = _context.WorkOrderStatus.Find(model.WorkOrderStatusId);//Estatus 2 es en modificación

                    workorder.Active = model.Active;
                    workorder.BuyerContactId = model.BuyerContactId;
                    workorder.Comments = model.Comments;
                    workorder.Customer = customer;
                    workorder.CustomerPO = model.CustomerPO;
                    workorder.FinalUserId = model.FinalUserId;
                    workorder.ModifiedBy = User.Identity.Name;
                    workorder.ModifyDate = DateTime.Now;
                    workorder.WorkOrderDate = model.WorkOrderDate;
                    workorder.Seller = model.SellerId;
                    workorder.WorkOrderName = model.WorkOrderName;
                    workorder.Tax = model.Tax;
                    workorder.PromiseDate = model.PromiseDate;
                    workorder.Currency = await _context.Currencies.FindAsync(model.CurrencyId);
                    workorder.ExchangeRate = model.ExchangeRate;
                    workorder.CreatedBy = await _userHelper.GetUserAsync(model.CreatedBy);
                    workorder.WorkOrderStatus = workorderstatus;
                    _context.Update(workorder);
                    await _context.SaveChangesAsync();
                    TempData["EditWorkOrderResult"] = "true";
                    TempData["EditWorkOrderMessage"] = "La Orden de trabajo fué actualizada";
                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllWorkOrders", _context.WorkOrders.ToList()) });

                }
                catch (DbUpdateException dbUpdateException)
                {


                    ModelState.AddModelError(string.Empty, dbUpdateException.Message);
                    return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditWorkOrder", model) });

                }

            }
            else
            {

                WorkOrder workorder = await _context.WorkOrders
                    .Include(w => w.Customer)
                    .Include(w => w.WorkOrderStatus)
                .Include(w => w.Currency)
                    .FirstOrDefaultAsync(w => w.WorkOrderId == model.WorkOrderId);
                //-----------------------

                List<SelectListItem> sellerlist = (List<SelectListItem>)await _userHelper.GetSellersAsync();
                List<SelectListItem> users = new List<SelectListItem>();
                List<SelectListItem> sellers = new List<SelectListItem>();
                List<SelectListItem> workorderstatus = new List<SelectListItem>();
                workorderstatus = _context.WorkOrderStatus
        .Select(w => new SelectListItem
        {
            Text = w.WorkOrderStatusName,
            Value = w.WorkOrderStatusId.ToString()
        }).OrderBy(c => c.Value).ToList();


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
                List<SelectListItem> currencies = (List<SelectListItem>)await _comboHelper.GetComboCurrenciesAsync();


                EditWorkOrder editworkorder = new EditWorkOrder()
                {
                    WorkOrderId = workorder.WorkOrderId,
                    WorkOrderName = model.WorkOrderName,
                    WorkOrderDate = model.WorkOrderDate,
                    PromiseDate = model.PromiseDate,
                    Customers = customers,
                    CustomerId = model.CustomerId,
                    CustomerPO = workorder.CustomerPO,
                    CustomerBuyerContacts = customerbuyercontacts,
                    BuyerContactId = model.BuyerContactId,
                    CustomerFinalContacts = customerfinalcontacts,
                    FinalUserId = model.FinalUserId,
                    Sellers = sellers,
                    SellerId = workorder.Seller,
                    Tax = model.Tax,
                    Active = true,
                    ModifiedBy = (List<SelectListItem>)await _userHelper.GetAllUsersAsync(),
                    ModifiedById = workorder.ModifiedBy,
                    ModifyDate = workorder.ModifyDate,
                    WorkOrderStatus = workorderstatus,
                    WorkOrderStatusId = workorder.WorkOrderStatus.WorkOrderStatusId,
                    Comments = workorder.Comments,
                    CurrencyId = workorder.Currency.CurrencyId,
                    Currency = currencies,
                    ExchangeRate = workorder.ExchangeRate
                };

                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditWorkOrder", editworkorder) });
            }

        }

        public async Task<IActionResult> AddOrEditDetail(int id = 0, int detailid = 0)
        {


            //Buscamos la cotización para pasar todos sus valores al Detalle

            WorkOrderDetail wd = new WorkOrderDetail();

            wd.WorkOrder = await _context.WorkOrders.FindAsync(id);


            //El Detalle siempre traerá un id de cotización ya que sea nuevo el detalle o actualización la Quote ya existe
            WorkOrderDetailViewModel wdv = new WorkOrderDetailViewModel();


            //Pasamos el listado de unidades para que aparezca en un combo despues asignaremos un default si el 
            //movimiento es un Edit si es un nuevo detalle entonces se quedeará como esta sin asignar el id de la unidad


            if (detailid == 0)
            {
                List<SelectListItem> Unit = (List<SelectListItem>)await _comboHelper.GetComboUnitAsync();
                List<SelectListItem> Category = (List<SelectListItem>)await _comboHelper.GetComboCategoriesAsync();
                List<SelectListItem> MaterialType = (List<SelectListItem>)await _comboHelper.GetComboMaterialTypesAsync(0);
                List<SelectListItem> Material = (List<SelectListItem>)await _comboHelper.GetComboMaterialsAsync(0);
                wdv = new()
                {
                    WorkOrderId = wd.WorkOrder.WorkOrderId,
                    Category = Category,
                    MaterialType = MaterialType,
                    Unit = Unit,

                };
                return View(wdv);
            }
            else
            {
                wd = await _context.WorkOrderDetails
                           .Include(w => w.WorkOrder)
                           .Include(w => w.Material).ThenInclude(m => m.MaterialType).ThenInclude(m => m.Category)
                           .Include(w => w.Unit)
                           .FirstOrDefaultAsync(q => q.WorkOrderDetailId == detailid);

                List<SelectListItem> Unit = (List<SelectListItem>)await _comboHelper.GetComboUnitAsync();
                List<SelectListItem> Category = (List<SelectListItem>)await _comboHelper.GetComboCategoriesAsync();
                List<SelectListItem> MaterialType = (List<SelectListItem>)await _comboHelper.GetComboMaterialTypesAsync(wd.Material.MaterialType.Category.CategoryId);
                List<SelectListItem> Material = (List<SelectListItem>)await _comboHelper.GetComboMaterialsAsync(wd.Material.MaterialType.MaterialTypeId);

                bool completed = false;

                //El campo completed va ayudar a tener un indice por ese campo para aquellos registros que vamos a descartar en la Carga

                if (wd.Quantity == wd.RawMaterial && wd.Quantity == wd.Machined && wd.Quantity == wd.Shipped && wd.Quantity == wd.Invoiced)
                {
                    completed = true;
                }

                wdv = new()
                {
                    WorkOrderId = wd.WorkOrder.WorkOrderId,
                    WorkOrderDetailId = wd.WorkOrderDetailId,
                    Category = Category,
                    CategoryId = wd.Material.MaterialType.Category.CategoryId,
                    MaterialTypeId = wd.Material.MaterialType.MaterialTypeId,
                    MaterialType = MaterialType,
                    MaterialId = wd.Material.MaterialId,
                    Material = Material,
                    Quantity = wd.Quantity,
                    Price = wd.Price,
                    Unit = Unit,
                    UnitId = wd.Unit.UnitId,
                    Description = wd.Description,
                    RawMaterial = wd.RawMaterial,
                    Machined = wd.Machined,
                    TT = wd.TT,
                    Shipped = wd.Shipped,
                    Invoiced = wd.Invoiced,
                    Completed = completed
                };


            };

            return View(wdv);
        }

        [HttpPost]
        public async Task<IActionResult> CopyDetail(int workOrderDetailId)
        {

            try
            {
                WorkOrderDetail wod = await _context.WorkOrderDetails
                .Include(w => w.WorkOrder)
                .Include(w => w.Unit)
                .Include(w => w.Material)
                .FirstOrDefaultAsync(w => w.WorkOrderDetailId == workOrderDetailId);

                WorkOrderDetail wodn = new WorkOrderDetail();
                wodn.WorkOrder = wod.WorkOrder;
                wodn.Description = wod.Description;
                wodn.Quantity = wod.Quantity;
                wodn.Material = wod.Material;
                wodn.Price = wod.Price;
                wodn.WorkOrderDetailComments = wod.WorkOrderDetailComments;
                wodn.WorkOrderDetailProcess = wod.WorkOrderDetailProcess;
                wodn.Unit = wod.Unit;

                _context.Add(wodn);
                await _context.SaveChangesAsync();

                return Json(new { data = "success", isValid = true });
            }
            catch (Exception)
            {

                return View();

            }





        }

        

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int workorderid, int statusid)
        {

            try
            {
                WorkOrder workorder = _context.WorkOrders.Find(workorderid);
                WorkOrderStatus workorderstatus = _context.WorkOrderStatus.Find(statusid);
                workorder.WorkOrderStatus = workorderstatus;
                _context.Update(workorder);
                await _context.SaveChangesAsync();
                return Json(new { data = "success", isValid = true });
            }
            catch (Exception)
            {

                return View();

            }





        }

        [HttpPost]
        public async Task<IActionResult> AddOrEditDetail(WorkOrderDetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.WorkOrderDetailId == 0)
                {
                    try
                    {
                        WorkOrderDetail wd = new WorkOrderDetail();
                        wd.WorkOrder = await _context.WorkOrders.FindAsync(model.WorkOrderId); ;
                        wd.Description = model.Description;
                        wd.Material = await _context.Materials.FindAsync(model.MaterialId);
                        wd.Price = model.Price;
                        wd.Quantity = model.Quantity;
                        wd.RawMaterial = model.RawMaterial;
                        wd.TT = model.TT;
                        wd.Machined = model.Machined;
                        wd.Invoiced = model.Invoiced;
                        wd.Shipped = model.Shipped;
                        wd.Unit = await _context.Units.FindAsync(model.UnitId);

                        _context.Add(wd);

                        WorkOrder q = await _context.WorkOrders.FindAsync(wd.WorkOrder.WorkOrderId);
                        q.WorkOrderStatus = await _context.WorkOrderStatus.FindAsync(2);
                        _context.Update(q);

                        await _context.SaveChangesAsync();


                        TempData["AddOrEditWorkOrderDetailResult"] = "true";
                        TempData["AddOrEditWorkOrderDetailMessage"] = "La partida fué creada";
                        return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllWorkOrderDetails", _context.WorkOrderDetails.Where(q => q.WorkOrder.WorkOrderId == model.WorkOrderId).ToList()) });

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
                        WorkOrderDetail wd = await _context.WorkOrderDetails.FindAsync(model.WorkOrderDetailId);

                        wd.WorkOrder = await _context.WorkOrders.FindAsync(model.WorkOrderId); ;
                        wd.Description = model.Description;
                        wd.Material = await _context.Materials.FindAsync(model.MaterialId);
                        wd.Price = model.Price;
                        wd.Quantity = model.Quantity;
                        wd.Unit = await _context.Units.FindAsync(model.UnitId);
                        wd.RawMaterial = model.RawMaterial;
                        wd.Machined = model.Machined;
                        wd.TT = model.TT;
                        wd.Invoiced = model.Invoiced;
                        wd.Shipped = model.Shipped;

                        if (model.WorkOrderDetailId != 0)
                        {
                            _context.Update(wd);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            _context.Add(wd);
                            await _context.SaveChangesAsync();
                        }

                        /*
                          Nos traemos todos los detalles

                         */
                        List<WorkOrderDetail> list = new List<WorkOrderDetail>();
                        list = _context.WorkOrders.FirstOrDefault(x => x.WorkOrderId == model.WorkOrderId).WorkOrderDetails.ToList();
                        List<AllWorkOrderDetails> details = new List<AllWorkOrderDetails>().ToList();

                        foreach (var detail in list)
                        {
                            AllWorkOrderDetails detailsv = new()
                            {
                                WorkOrderDetailId = detail.WorkOrderDetailId,
                                WorkOrder = detail.WorkOrder,
                                Description = detail.Description,
                                Material = detail.Material,
                                MaterialName = detail.Material.MaterialName,
                                Price = detail.Price,
                                Quantity = detail.Quantity,
                                Unit = detail.Unit,
                                UnitName = detail.Unit.UnitName,
                                RawMaterial = detail.RawMaterial,
                                Machined = detail.Machined,
                                TT = detail.TT,
                                Invoiced = detail.Invoiced,
                                Shipped = detail.Shipped

                            };
                            details.Add(detailsv);
                        }


                        return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllWorkOrderDetails", details) });
                    }
                    catch (Exception)
                    {

                        return View(model);

                    }
                }


            }
            else
            {

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

        public JsonResult GetBuyerContacts(int customerId)
        {

            List<SelectListItem> Contacts = new List<SelectListItem>();

            Contacts = _context.Contacts
                   .Where(c => c.Customer.CustomerId == customerId)
                   .Select(c => new SelectListItem
                   {
                       Text = c.Name + " " + c.LastName,
                       Value = c.ContactId.ToString()
                   })
                  .OrderBy(c => c.Text)
                  .ToList();

            return Json(Contacts);

        }

        public ActionResult AddOrEditComment(int id)
        {
            List<WorkOrderCommentView> comments = _context.WorkOrderComments
                .Include(c => c.User)
                .Where(c => c.WorkOrder.WorkOrderId == id)
                .Select(c => new WorkOrderCommentView
                {
                    UserName = c.User.Email,
                    CommentId = c.CommentId,
                    WorkOrderId = c.WorkOrder.WorkOrderId,
                    Comment = c.Comment,
                    Usuario = c.User.FullName,
                    DateComment = c.DateComment
                })
                .OrderBy(c => c.WorkOrderId)
                .ToList();

            ViewBag.WorkOrderId = id;
            var name = User.Identity.Name;
            User usr = _context.Users.Where(u => u.UserName == name).FirstOrDefault();
            ViewBag.UserName = name;
            ViewBag.Usuario = usr.FullName;
            //ViewBag.UserName = "@" + name.Substring(0, name.IndexOf("@"));



            return View(comments);


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEditComment(int WorkOrderId, WorkOrderCommentView model)
        {
            if (ModelState.IsValid)
            {

                try
                {

                    WorkOrderComment comment = new WorkOrderComment()
                    {
                        WorkOrder = await _context.WorkOrders.FindAsync(WorkOrderId),
                        Comment = model.Comment,
                        DateComment = DateTime.Now,
                        User = await _userHelper.GetUserAsync(User.Identity.Name)
                    };

                    _context.Add(comment);
                    await _context.SaveChangesAsync();
                    TempData["AddOrEditCommentResult"] = "true";
                    TempData["AddOrEditCommentMessage"] = "El comentario fué agregado";

                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllWorkOrders", _context.WorkOrders.ToList()) });

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

        [HttpGet]
        public async Task<IActionResult> DeleteWorkOrderDetail(int id)
        {
            WorkOrderDetail wod = await _context.WorkOrderDetails
               .Include(d => d.WorkOrder)
               .FirstOrDefaultAsync(d => d.WorkOrderDetailId == id);
            int workOrderId = wod.WorkOrder.WorkOrderId;

            return View(wod);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWorkOrderDetail(WorkOrderDetail model)
        {


            try
            {
                //Borrar archivos

                int workorderid = model.WorkOrder.WorkOrderId;
                List<WorkOrderDetailComment> lwodc = new List<WorkOrderDetailComment>();

                List<Archive> archives = _context.Archives.Where(a => a.Entity == "WorkOrderDetail" && a.EntityId == model.WorkOrderDetailId).ToList();

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

                lwodc = _context.WorkOrderDetailComments.Where(c => c.WorkOrderDetail.WorkOrderDetailId == model.WorkOrderDetailId).ToList();

                foreach (var comment in lwodc)
                {
                    _context.Remove(comment);
                }
                await _context.SaveChangesAsync();

                _context.WorkOrderDetails.Remove(model);
                await _context.SaveChangesAsync();

                TempData["DeleteWorkOrderDetailResult"] = "true";
                TempData["DeleteWorkOrderDetailMessage"] = "La partida fue eliminada";
            }

            catch
            {
                TempData["DeleteWorkOrderDetailResult"] = "false";
                TempData["DeleteWorkOrderDetailMessage"] = "La partida no pudo ser eliminada";
                return RedirectToAction(nameof(Details), new { id = model.WorkOrder.WorkOrderId });

            }

            return RedirectToAction(nameof(Details), new { id = model.WorkOrder.WorkOrderId });

        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id)
        {
            WorkOrderComment comment = await _context.WorkOrderComments.FindAsync(id);
            try
            {
                _context.WorkOrderComments.Remove(comment);
                await _context.SaveChangesAsync();
                TempData["DeleteCommentResult"] = "true";
                TempData["DeleteCommentMessage"] = "El comentario fué eliminado";
                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllWorkOrders", _context.WorkOrders.ToList()) });


            }
            catch (Exception e)
            {
                TempData["DeleteCommentResult"] = "false";
                TempData["EeleteCommentMessage"] = "El comentario no pudo ser eliminado error: " + e.Message;
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllWorkOrders", _context.WorkOrders.ToList()) });

            }


        }


        public async Task<FileResult> PrintReport(int WorkOrderId)
        {

            WorkOrder workorder = await _context.WorkOrders.FindAsync(WorkOrderId);

            Stream stream = new MemoryStream(await _reportHelper.GenerateWorkOrderReportPDFAsync(WorkOrderId));

            return File(stream, "application/pdf", $"{workorder.WorkOrderName}{".pdf"}");
        }

        public async Task<FileResult> Export()
        {

            DataTable dt = new DataTable("WorkOrderResult");
            dt.Columns.AddRange(new DataColumn[19] {
                                            new DataColumn("OT",Type.GetType("System.String")),
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
                                            new DataColumn("Precio",Type.GetType("System.Decimal")),
                                            new DataColumn("MP",Type.GetType("System.Int64")),
                                            new DataColumn("Maquinadas",Type.GetType("System.Int64")),
                                            new DataColumn("TT",Type.GetType("System.Int64")),
                                            new DataColumn("Embarcadas",Type.GetType("System.Int64")),
                                            new DataColumn("Facturadas",Type.GetType("System.Int64"))
                                            });

            List<WorkOrder> workorders = await _context.WorkOrders
               .Include(w => w.WorkOrderDetails).ThenInclude(d => d.Material)
               .Include(w => w.WorkOrderDetails).ThenInclude(d => d.Unit)
               .Include(w => w.Customer)
               .Include(w => w.WorkOrderStatus)
               .Include(w => w.Currency)
               .OrderByDescending(w => w.QuoteId)
               .ToListAsync();

            foreach (WorkOrder workorder in workorders)
            {
                string seller = _userHelper.GetUserAsync(workorder.Seller).Result.FullName.ToString();
                Contact buyercontact = _context.Contacts.Find(workorder.BuyerContactId);
                Contact finaluser = _context.Contacts.Find(workorder.FinalUserId);
                foreach (WorkOrderDetail wd in workorder.WorkOrderDetails)
                {
                    dt.Rows.Add(workorder.WorkOrderName,
                                workorder.Customer.CustomerNickName,
                                $"{finaluser.Name} {finaluser.LastName}",
                                workorder.Active.ToString(),
                                $"{buyercontact.Name} {buyercontact.LastName}",
                                workorder.CreatedBy,
                                workorder.Currency.Curr,
                                workorder.CustomerPO,
                                workorder.Seller,
                                wd.Material.MaterialName,
                                wd.Description,
                                wd.Quantity,
                                wd.Unit.UnitName,
                                wd.Price,
                                wd.RawMaterial,
                                wd.Machined,
                                wd.TT,
                                wd.Shipped,
                                wd.Invoiced
                                ); ;
                }
            }

            try
            {
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dt, "Ots");
                    using (MemoryStream stream = new MemoryStream())
                    {
                        string exportfilename = $"WorkOrders{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
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

        public async Task<FileResult> ExportWorkLoad()
        {
            List<WorkLoad> lstworkload = new List<WorkLoad>();


            List<WorkOrder> workOrders = await _context.WorkOrders
                .Include(w => w.Customer)
                .Include(w => w.WorkOrderStatus)
                .Include(w => w.Currency)
                .Include(w => w.WorkOrderDetails).ThenInclude(d => d.Material)
                .Include(w => w.WorkOrderDetails).ThenInclude(d => d.Unit)
                .Include(w => w.WorkOrderDetails).ThenInclude(d => d.WorkOrderDetailProcess).ThenInclude(p => p.Machine).ThenInclude(m => m.Process)
                .Include(w => w.WorkOrderDetails).ThenInclude(d => d.WorkOrderDetailProcess).ThenInclude(p => p.Unit)
                .Where(w => w.WorkOrderStatus.WorkOrderStatusId == 1
                         || w.WorkOrderStatus.WorkOrderStatusId == 2
                         || w.WorkOrderStatus.WorkOrderStatusId == 3
                         || w.WorkOrderStatus.WorkOrderStatusId == 4
                         || w.WorkOrderStatus.WorkOrderStatusId == 5
                         || w.WorkOrderStatus.WorkOrderStatusId == 6).ToListAsync();

            foreach (WorkOrder wo in workOrders)
            {

               

                foreach (WorkOrderDetail wod in wo.WorkOrderDetails)
                {
                    WorkLoad workload = new WorkLoad();
                    var quotename = "";
                    int quoteid = 0 ;

                    if (wo.QuoteId != null)
                    {
                        quoteid = wo.QuoteId.Value;
                        try
                        {
                            quotename = _context.Quotes.FindAsync(wo.QuoteId).Result.QuoteName;
                        }
                        catch (Exception)
                        {
                            quotename = "";
                            quoteid = 0;
                            throw;
                        }
                    }

                    


                    Contact contact = await _context.Contacts.FindAsync(wo.FinalUserId);
                    Contact buyer = await _context.Contacts.FindAsync(wo.BuyerContactId);

                    workload.WorkOrderId = wo.WorkOrderId;
                    workload.QuoteId = quoteid;
                    workload.WorkOrderName = wo.WorkOrderName;
                    workload.QuoteName = quotename;
                    workload.WorkOrderStatusName = wo.WorkOrderStatus.WorkOrderStatusName;
                    workload.CustomerNickName = wo.Customer.CustomerNickName;
                    workload.CustomerPO = wo.CustomerPO;
                    workload.FinalUser = $"{contact.Name} {contact.LastName}";
                    workload.Buyer = $"{buyer.Name} {buyer.LastName}";
                    workload.Seller = wo.Seller;
                    workload.PODate = wo.WorkOrderDate;//Este hay que corregirlo
                    workload.PromiseDate = wo.PromiseDate;
                    workload.ShippedDate = wo.ModifyDate; //Este hay que corregirlo ya que solo tendrá valor hasta que se haya entregado toda la
                    workload.Currency = wo.Currency.Curr;
                    workload.ExchangeRate = wo.ExchangeRate;
                    string process = "";
                    workload.WorkOrderDetailId = wod.WorkOrderDetailId;
                    workload.Description = wod.Description;
                    workload.Quantity = wod.Quantity;
                    workload.Price = wod.Price;
                    workload.MaterialName = wod.Material.MaterialName;
                    workload.UnitName = wod.Unit.UnitName;
                    workload.RawMaterial = wod.RawMaterial;
                    workload.Machined = wod.Machined;
                    workload.TT = wod.TT;
                    workload.Shipped = wod.Shipped;
                    workload.Invoiced = wod.Invoiced;


                    if (wod.WorkOrderDetailProcess != null)
                    {
                        foreach (WorkOrderDetailProcess p in wod.WorkOrderDetailProcess)
                        {
                            string machinename = "";
                            string processname = "";

                            machinename = p.Machine.MachineName;
                            processname = p.Machine.Process.ProcessName;
                            process += $"{p.Machine.MachineName}-{p.Machine.Process.ProcessName}-{p.Quantity}-{p.Unit.UnitName}|";
                        }
                        if (process.Length > 0)
                        {
                            process = process.Substring(0, process.Length - 1);
                        }
                    }
                    workload.ProcessArray = process;
                    lstworkload.Add(workload);
                }
            }
         

            DataTable dt = new DataTable("WorkLoadResult");
            dt.Columns.AddRange(new DataColumn[22] {
                                            new DataColumn("ID WOD",Type.GetType("System.Int64")),
                                            new DataColumn("OT",Type.GetType("System.String")),
                                            new DataColumn("Cotización",Type.GetType("System.String")),
                                            new DataColumn("Cliente",Type.GetType("System.String")),
                                            new DataColumn("Estatus OT",Type.GetType("System.String")),
                                            new DataColumn("OC",Type.GetType("System.String")),
                                            new DataColumn("Usuario",Type.GetType("System.String")),
                                            new DataColumn("Comprador", Type.GetType("System.String")),
                                            new DataColumn("Vendedor", Type.GetType("System.String")),
                                            new DataColumn("Fecha OC", Type.GetType("System.String")),
                                            new DataColumn("Fecha Compromiso", Type.GetType("System.String")),
                                            new DataColumn("Fecha Embarque", Type.GetType("System.String")),
                                            new DataColumn("Descripción",Type.GetType("System.String")),
                                            new DataColumn("Cantidad",Type.GetType("System.Decimal")),
                                            new DataColumn("Precio",Type.GetType("System.Decimal")),
                                            new DataColumn("Total",Type.GetType("System.Decimal")),
                                            new DataColumn("Moneda", Type.GetType("System.String")),
                                            new DataColumn("MP",Type.GetType("System.Int64")),
                                            new DataColumn("Maq",Type.GetType("System.Int64")),
                                            new DataColumn("TT",Type.GetType("System.Int64")),
                                            new DataColumn("Embarcadas",Type.GetType("System.Int64")),
                                            new DataColumn("Facturadas",Type.GetType("System.Int64"))
                                            });



            foreach (WorkLoad w in lstworkload)
            {
         
                    dt.Rows.Add(w.WorkOrderDetailId,
                                w.WorkOrderName,
                                w.QuoteName,
                                w.CustomerNickName,
                                w.WorkOrderStatusName,
                                w.CustomerPO,
                                w.FinalUser,
                                w.Buyer,
                                w.Seller,
                                w.PODate,
                                w.PromiseDate,
                                w.ShippedDate,
                                w.Description,
                                w.Quantity,
                                w.Price,
                                w.Total,
                                w.Currency,
                                w.RawMaterial,
                                w.Machined,
                                w.TT,
                                w.Shipped,
                                w.Invoiced
                                ); ;
                
            }

            try
            {
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dt, "Carga");
                    using (MemoryStream stream = new MemoryStream())
                    {
                        string exportfilename = $"Carga{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
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

        public ActionResult AddOrEditDetailComment(int id)
        {
            List<WorkOrderDetailCommentView> comments = _context.WorkOrderDetailComments
                .Include(c => c.User)
                .Where(c => c.WorkOrderDetail.WorkOrderDetailId == id)
                .Select(c => new WorkOrderDetailCommentView
                {
                    UserName = c.User.Email,
                    CommentId = c.CommentId,
                    WorkOrderDetailId = c.WorkOrderDetail.WorkOrderDetailId,
                    Comment = c.Comment,
                    Usuario = c.User.FullName,
                    DateComment = c.DateComment
                })
                .OrderBy(c => c.WorkOrderDetailId)
                .ToList();

            ViewBag.WorkOrderDetailId = id;
            var name = User.Identity.Name;
            User usr = _context.Users.Where(u => u.UserName == name).FirstOrDefault();
            ViewBag.UserName = name;
            ViewBag.Usuario = usr.FullName;

            return View(comments);


        }

        public async Task<IActionResult> AddOrEditProcess(int id, int workOrderDetailid)
        {

           
            if (id > 0)
            {
                WorkOrderDetailProcess wodp = await _context.WorkOrderDetailProcesses
                                                            .Include(w => w.WorkOrderDetail)
                                                            .Include(w => w.Machine).ThenInclude(w => w.Process)
                                                            .Include(w => w.Unit)
                                                            .FirstOrDefaultAsync(w => w.WorkOrderDetailProcessId == id);

                WorkOrderDetailProcessViewModel workOrderDetailProcessViewModel = new()
                {
                    WorkOrderDetailProcessId = wodp.WorkOrderDetailProcessId,
                    WorkOrderDetailId = wodp.WorkOrderDetail.WorkOrderDetailId,
                    Process = await _comboHelper.GetComboProcessAsync(wodp.Machine.Process.ProcessId),
                    Machines = await _comboHelper.GetComboMachinesAsync(wodp.Machine.MachineId),
                    MachineId = wodp.Machine.MachineId,
                    ProcessId = wodp.Machine.Process.ProcessId,
                    Units = await _comboHelper.GetComboUnitAsync(wodp.Unit.UnitId),
                    UnitId = wodp.Unit.UnitId,
                    Quantity = wodp.Quantity
                };

                return View(workOrderDetailProcessViewModel);
            }
            else 
            {
                WorkOrderDetailProcessViewModel workOrderDetailProcessViewModel = new() 
                { 
                   WorkOrderDetailProcessId = 0,
                   WorkOrderDetailId = workOrderDetailid,
                   Process = await _comboHelper.GetComboProcessAsync(),
                   Machines = await _comboHelper.GetComboMachinesAsync(),
                   Units = await _comboHelper.GetComboUnitAsync(),
                   Quantity = 0
                };

                return View(workOrderDetailProcessViewModel);
            }

        }

        [HttpPost]
        public async Task<IActionResult> AddOrEditProcess(WorkOrderDetailProcessViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.WorkOrderDetailProcessId == 0)
                {
                    try
                    {
                        WorkOrderDetailProcess wodp = new WorkOrderDetailProcess();
                        wodp.WorkOrderDetail = await _context.WorkOrderDetails
                                                    .Include(w => w.WorkOrder)
                                                    .FirstOrDefaultAsync(w => w.WorkOrderDetailId == model.WorkOrderDetailId);
                        wodp.Machine = await _context.Machines.FindAsync(model.MachineId);
                        wodp.Unit = await _context.Units.FindAsync(model.UnitId);
                        wodp.Quantity = model.Quantity; 
                        _context.Add(wodp);
                        await _context.SaveChangesAsync();


                        TempData["AddOrEditWorkOrderDetailProcessResult"] = "true";
                        TempData["AddOrEditWorkOrderDetailMessage"] = "El proceso fué agregado";
                        return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllWorkOrderDetails", _context.WorkOrderDetails.Where(q => q.WorkOrder.WorkOrderId == wodp.WorkOrderDetail.WorkOrder.WorkOrderId).ToList()) });

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
                        WorkOrderDetailProcess wodp = await _context.WorkOrderDetailProcesses
                            .Include(w => w.WorkOrderDetail).ThenInclude(w => w.WorkOrder)  
                            .FirstOrDefaultAsync(w => w.WorkOrderDetailProcessId == model.WorkOrderDetailProcessId);
                        wodp.Machine = await _context.Machines.FindAsync(model.MachineId);
                        wodp.Unit = await _context.Units.FindAsync(model.UnitId);
                        wodp.Quantity = model.Quantity;
                        _context.Update(wodp);

                        await _context.SaveChangesAsync();
                      


                        TempData["AddOrEditWorkOrderDetailProcessResult"] = "true";
                        TempData["AddOrEditWorkOrderDetailMessage"] = "El proceso fué actualizado";
                        return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllWorkOrderDetails", _context.WorkOrderDetails.Where(w => w.WorkOrder.WorkOrderId == wodp.WorkOrderDetail.WorkOrder.WorkOrderId).ToList()) });

                    }
                    catch (Exception)
                    {

                        return View(model);

                    }
                }


            }
            else
            {

                {
                   

                    return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEditDetail", model) });
                }
            }

        }

        public JsonResult GetMachines(int processId)
        {
            Process process = _context.Processes
                .Include(p => p.Machines)
                .FirstOrDefault(c => c.ProcessId == processId);
            if (process == null)
            {
                return null;
            }

            return Json(process.Machines.OrderBy(p => p.MachineName));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteProcess(int id)
        {
            WorkOrderDetailProcess process = await _context.WorkOrderDetailProcesses.FindAsync(id);
            return View(process);
        }

        [NoDirectAccess]
        [HttpPost]
        public async Task<IActionResult> DeleteProcess(WorkOrderDetailProcess model)
        {
   
            var process = await _context.WorkOrderDetailProcesses
                .Include(w => w.WorkOrderDetail).ThenInclude(d => d.WorkOrder)
                .FirstOrDefaultAsync(c => c.WorkOrderDetailProcessId == model.WorkOrderDetailProcessId);
            try
            {
                _context.WorkOrderDetailProcesses.Remove(process);
                await _context.SaveChangesAsync();
                TempData["WorkOrderDetailProcessDeleteResult"] = "true";
                TempData["WorkOrderDetailProcessDeleteMessage"] = "El proceso fué eliminado";

                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllWorkOrderDetails", _context.WorkOrderDetails.Where(w => w.WorkOrder.WorkOrderId == process.WorkOrderDetail.WorkOrder.WorkOrderId).ToList()) });

            }

            catch (Exception e)
            {
                TempData["WorkOrderDetailProcessDeleteResult"] = "false";
                TempData["WorkOrderDetailProcessDeleteMessage"] = "El proceso no pudo ser eliminado " + e.Message;
                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllWorkOrderDetails", _context.WorkOrderDetails.Where(w => w.WorkOrder.WorkOrderId == process.WorkOrderDetail.WorkOrder.WorkOrderId).ToList()) });

            }

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEditDetailComment(WorkOrderDetailCommentView model)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    WorkOrderDetail wd = await _context.WorkOrderDetails
                        .Include(w => w.WorkOrder)
                        .FirstOrDefaultAsync(w => w.WorkOrderDetailId == model.WorkOrderDetailId);

                    WorkOrderDetailComment comment = new WorkOrderDetailComment()
                    {
                        WorkOrderDetail = await _context.WorkOrderDetails.FindAsync(model.WorkOrderDetailId),
                        Comment = model.Comment,
                        DateComment = DateTime.Now,
                        User = await _userHelper.GetUserAsync(User.Identity.Name)
                    };

                    _context.Add(comment);
                    await _context.SaveChangesAsync();
                    TempData["AddOrEditDetailCommentResult"] = "true";
                    TempData["AddOrEditDetailCommentMessage"] = "El comentario fué agregado";

                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllWorkOrderDetails", _context.WorkOrderDetails.Where(w => w.WorkOrder.WorkOrderId == wd.WorkOrder.WorkOrderId).ToList()) });

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
        private bool WorkOrderExists(int id)
        {
            return _context.WorkOrders.Any(e => e.WorkOrderId == id);
        }
    }
}
