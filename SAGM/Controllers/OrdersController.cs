using Microsoft.AspNetCore.Mvc;
using SAGM.Data;
using SAGM.Helpers;
using SAGM.Models;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using SAGM.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.Reflection.PortableExecutable;
using DocumentFormat.OpenXml.InkML;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using SAGM.Migrations;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;

namespace SAGM.Controllers
{

    [Authorize(Roles = "Administrador,Comprador")]
    public class OrdersController : Controller
    {
        private readonly SAGMContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IComboHelper _comboHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IConfiguration _configuration;
        private readonly IReportHelper _reportHelper;

        public OrdersController(SAGMContext context, IUserHelper userHelper, IComboHelper comboHelper, IBlobHelper blobHelper, IConfiguration configuration, IReportHelper reportHelper)
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


            if (TempData["AddOrderResult"] != null)
            {

                ViewBag.Result = TempData["AddOrderResult"].ToString();
                ViewBag.Message = TempData["AddOrderMessage"].ToString();
                TempData.Remove("AddOrderResult");
                TempData.Remove("AddOrderMessage");
            };

            if (TempData["EditOrderResult"] != null)
            {

                ViewBag.Result = TempData["EditOrderResult"].ToString();
                ViewBag.Message = TempData["EditOrderMessage"].ToString();
                TempData.Remove("EditOrderResult");
                TempData.Remove("EditOrderMessage");
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

            if (TempData["CopyOrderResult"] != null)
            {
                ViewBag.Result = TempData["CopyOrderResult"].ToString();
                ViewBag.Message = TempData["CopyOrderMessage"].ToString();
                TempData.Remove("CopyOrderResult");
                TempData.Remove("CopyOrderMessage");
            }

            return View();

        }

        [HttpGet]
        public async Task<JsonResult> GetOrders()
        {
            ViewBag.Result = "";
            ViewBag.Message = "";

            List<Order> orders = await _context.Orders
                 .Include(o => o.OrderDetails)
                 .ThenInclude(o => o.Material)
                 .Include(o => o.Supplier)
                 .Include(o => o.OrderStatus)
                 .Include(o => o.Currency)
                 .Include(o => o.WorkOrder)
                 .Include(o => o.CreatedBy)
                 .OrderByDescending(o => o.OrderId)
                 .ToListAsync();

            List<OrderViewIndexModel> lorders = new List<OrderViewIndexModel>();

            foreach (Order o in orders)
            {
                string buyer = _userHelper.GetUserAsync(o.Buyer).Result.FullName.ToString();
                Contact sellercontact = await _context.Contacts.FindAsync(o.SupplierContactId);
                int archivesnumber = 0;

                string woname = "";
                int workorderid  = 0;

                if (o.WorkOrder != null)
                {
                    woname = o.WorkOrder.WorkOrderName;
                    workorderid = o.WorkOrder.WorkOrderId;

                }
                else {
                    woname = "";
                    workorderid = 0;
                }
             


                //Armamos la lista de detalles
                List<OrderDetailViewIndexModel> details = new List<OrderDetailViewIndexModel>();
                foreach (OrderDetail od in o.OrderDetails)
                {
                    List<Archive> archives = _context.Archives.Where(a => a.Entity == "OrderDetail" && a.EntityId == od.OrderDetailId).ToList();
                    archivesnumber += archives.Count;
                    OrderDetailViewIndexModel qdvim = new OrderDetailViewIndexModel()
                    {
                        Quantity = od.Quantity,
                        Material = _context.Materials.FindAsync(od.Material.MaterialId).Result.MaterialName.ToString(),
                        Description = od.Description,
                        Price = od.Price
                    };
                    details.Add(qdvim);

                }

                //

                List<Archive> qarchives = _context.Archives.Where(a => a.Entity == "Order" && a.EntityId == o.OrderId).ToList();

                string archiveschain = "";

                foreach (var item in qarchives)
                {
                    archiveschain += item.ArchiveGuid.ToString() + "," + item.ArchiveName + "," + item.ArchiveId + "|";
                }
                if (archiveschain != "")
                {
                    archiveschain = archiveschain.Substring(0, archiveschain.Length - 1);
                };

                OrderViewIndexModel aos = new OrderViewIndexModel()
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    Active = o.Active,
                    SellerContact = $"{sellercontact.Name} {sellercontact.LastName}",
                    Comments = o.Comments,
                    CreatedBy = o.CreatedBy.FullName,
                    Currency = o.Currency.Curr,
                    SupplierNickName = o.Supplier.SupplierNickName,
                    SupplierQuote = o.SupplierQuote,
                    EstimatedDeliveryDate = o.EstimatedDeliveryDate,
                    OrderDetails = details,
                    OrderName = o.OrderName,
                    Buyer = buyer,
                    Tax = o.Tax,
                    OrderStatusName = o.OrderStatus.OrderStatusName,
                    DeliveryDate = o.DeliveryDate,
                    ArchivesNumber = archivesnumber,
                    ArchivesChain = archiveschain,
                    WorkOrderid = workorderid,
                    WorkOrderName = woname

                };
                lorders.Add(aos);
            }

            return Json(new { data = lorders });
        }


        public async Task<IActionResult> AddOrder()
        {
            String Lastnumber = ""; //Ultimo numero consecutivo de la OC
            int Consec = 0;

            TimeSpan estimatedDeliveryDate = new TimeSpan(3, 0, 0, 0); //Diez dias de vigencia por defecto

            String ordername = DateTime.Now.ToString("yyyyMMdd"); //Variable formadora de nombre de OC

            ordername = "OC-" + ordername + "-XXX";

            // -------------------------

            Order Lastorder = await _context.Orders.OrderBy(o => o.OrderId).LastOrDefaultAsync();//Ultima cotizacion

            if (Lastorder != null)
            {
                Lastnumber = Lastorder.OrderName.Substring(12, 3);
                Consec = Int32.Parse(Lastnumber);
            }
            else
            {
                Lastnumber = "000";
                Consec = Int32.Parse(Lastnumber);
            }

            //-----------------------

            List<SelectListItem> buyerlist = (List<SelectListItem>)await _userHelper.GetSellersAsync();
            List<SelectListItem> users = new List<SelectListItem>();
            List<SelectListItem> buyers = new List<SelectListItem>();


            users = _context.Users
                   .Where(u => u.EmailConfirmed == true)
                   .Select(u => new SelectListItem
                   {
                       Text = u.FirstName + " " + u.LastName,
                       Value = u.UserName
                   })
                  .OrderBy(c => c.Text)
                  .ToList();

            buyers = (List<SelectListItem>)(from u in users join s in buyerlist on u.Value equals s.Value select u).ToList();
            List<SelectListItem> currencies = (List<SelectListItem>)await _comboHelper.GetComboCurrenciesAsync(1);

            AddOrder order = new AddOrder()
            {
                OrderName = ordername,
                OrderDate = DateTime.Now,
                EstimatedDeliveryDate = DateTime.Now.Add(estimatedDeliveryDate),
                Suppliers = await _comboHelper.GetComboSuppliersAsync(),
                SupllierContacts = await _comboHelper.GetComboContactSuppliersAsync(0),
                Buyers = buyers,
                Tax = 16,
                Active = true,
                CurrencyId = 1,
                Currency = currencies
            };

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrder(AddOrder model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.OrderId == 0)
                    {
                        String ordername = DateTime.Now.ToString("yyyyMMdd"); //Variable formadora de nombre de coti
                        String Lastnumber = ""; //Ultimo numero consecutivo de la cotización
                        String strnumber = "";
                        int Consec = 0;


                        ordername = "OC-" + ordername;

                        // -------------------------

                        Order Lastorder = await _context.Orders.Where(o => o.OrderName.Substring(0, 11) == ordername).OrderBy(o => o.OrderId).LastOrDefaultAsync();//Ultima OC

                        if (Lastorder != null)
                        {
                            Lastnumber = Lastorder.OrderName.Substring(12, 3);
                            Consec = Int32.Parse(Lastnumber) + 1;
                        }
                        else
                        {
                            Lastnumber = "001";
                            Consec = Int32.Parse(Lastnumber);
                        }

                        Lastnumber = Consec.ToString();

                        strnumber = $"000{Lastnumber}";

                        ordername = ordername + "-" + strnumber.Substring(strnumber.Length - 3, 3);

                        //-----------------------
                        Contact supplierContact = await _context.Contacts.FindAsync(model.SupplierContactId);
                        User createdBy = await _userHelper.GetUserAsync(User.Identity.Name);
                        Currency currency = await _context.Currencies.FindAsync(model.CurrencyId);
                        Supplier supplier = await _context.Suppliers.FindAsync(model.SupplierId);
                        OrderStatus orderstatus = await _context.OrderStatus.FindAsync(1);//El primer estatus es creada

                        Order order = new Order()
                        {
                            Active = model.Active,
                            SupplierContactId = model.SupplierContactId,
                            Comments = model.Comments,
                            CreatedBy = createdBy,
                            Currency = await _context.Currencies.FindAsync(model.CurrencyId),
                            Supplier = supplier,
                            SupplierQuote = model.SupplierQuote,
                            OrderDate = model.OrderDate,
                            OrderName = ordername,
                            Buyer = model.BuyerId,
                            Tax = model.Tax,
                            EstimatedDeliveryDate = model.EstimatedDeliveryDate,
                            DeliveryDate = model.DeliveryDate,
                            OrderStatus = orderstatus,

                        };
                        _context.Add(order);
                        await _context.SaveChangesAsync();
                        TempData["AddOrderResult"] = "true";
                        TempData["AddOrderMessage"] = "La Orden de compra fué creada";
                        return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllOrders", _context.Quotes.ToList()) });
                    }

                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllOrders", _context.Quotes.ToList()) });

                }
                catch (DbUpdateException dbUpdateException)
                {


                    ModelState.AddModelError(string.Empty, dbUpdateException.Message);
                    return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrder", model) });

                }

            }
            else
            {

                List<SelectListItem> suppliers = (List<SelectListItem>)await _comboHelper.GetComboSuppliersAsync();
                List<SelectListItem> supplierContacts = new List<SelectListItem>();
                List<SelectListItem> buyerlist = (List<SelectListItem>)await _userHelper.GetBuyersAsync();
                List<SelectListItem> currencies = (List<SelectListItem>)await _comboHelper.GetComboCurrenciesAsync(model.CurrencyId);

                SelectListItem buyer = buyerlist.Where(b => b.Value.ToString() == model.BuyerId.ToString()).FirstOrDefault();
                buyer.Selected = true;

                if (model.SupplierId != 0)
                {
                    var supplieritem = suppliers.Where(s => s.Value.ToString() == model.SupplierId.ToString()).FirstOrDefault();
                    supplieritem.Selected = true;

                    supplierContacts = _context.Contacts
                      .Where(s => s.Supplier.SupplierId == model.SupplierId)
                      .Select(c => new SelectListItem
                      {
                          Text = c.Name + " " + c.LastName,
                          Value = c.ContactId.ToString()
                      })
                     .OrderBy(c => c.Text)
                     .ToList();
                    supplierContacts.Insert(0, new SelectListItem { Text = "[Seleccione un contacto...]", Value = "0" });


                    if (model.SupplierContactId != 0)
                    {
                        var buyercontact = supplierContacts.Where(c => c.Value.ToString() == model.SupplierContactId.ToString()).FirstOrDefault();
                        buyercontact.Selected = true;
                    }

                }
                else
                {
                    supplierContacts.Insert(0, new SelectListItem { Text = "[Seleccione un contacto...]", Value = "0" });
                }


                model.Buyers = buyerlist;
                model.Suppliers = suppliers;
                model.SupllierContacts = supplierContacts;
                model.Currency = currencies;
                model.CurrencyId = model.CurrencyId;

                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrder", model) });
            }

        }

        public JsonResult GetSupplierContacts(int supplierID)
        {

            List<SelectListItem> Contacts = new List<SelectListItem>();

            Contacts = _context.Contacts
                   .Where(c => c.Supplier.SupplierId == supplierID)
                   .Select(c => new SelectListItem
                   {
                       Text = c.Name + " " + c.LastName,
                       Value = c.ContactId.ToString()
                   })
                  .OrderBy(c => c.Text)
                  .ToList();

            return Json(Contacts);

        }

        public async Task<IActionResult> Details(int? id, int? orderDetilId)
        {


            ViewBag.Result = "";
            ViewBag.Message = "";
            ViewBag.orderDetailId = orderDetilId.ToString();




            if (TempData != null)
            {
                if (TempData["AddOrEditQuoteDetailResult"] != null)
                {

                    ViewBag.Result = TempData["AddOrEditOrderDetailResult"].ToString();
                    ViewBag.Message = TempData["AddOrEditOrderDetailMessage"].ToString();
                    ViewBag.quoteDetailId = orderDetilId.ToString();
                    TempData.Remove("AddOrEditOrderDetailResult");
                    TempData.Remove("AddOrEditOrderDetailMessage");
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
                if (TempData["DeleteOrderDetailtResult"] != null)
                {
                    ViewBag.Result = TempData["DeleteOrderDetailtResult"].ToString();
                    ViewBag.Message = TempData["DeleteOrderDetailMessage"].ToString();
                    TempData.Remove("DeleteOrderDetailtResult");
                    TempData.Remove("DeleteOrderDetailMessage");
                }
                if (TempData["ReceiptResult"] != null)
                {
                    ViewBag.Result = TempData["ReceiptResult"].ToString();
                    ViewBag.Message = TempData["ReceiptMessage"].ToString();
                    TempData.Remove("ReceiptResult");
                    TempData.Remove("ReceiptMessage");
                }


            }


            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Supplier)
                .Include(o => o.OrderDetails)
                .Include(o => o.OrderStatus)
                .Include(o => o.Currency)
                .Include(o => o.WorkOrder)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            int workorderid = 0;
            string workordername = "";

            if (order.WorkOrder != null)
            {
                workorderid = order.WorkOrder.WorkOrderId;
                workordername = order.WorkOrder.WorkOrderName;
            }

            ViewBag.DetailsCount = order.OrderDetails.Count();

            if (order == null)
            {
                return NotFound();
            }

            List<SelectListItem> orderstatus = (List<SelectListItem>)await _comboHelper.GetComboOrderStatus(order.OrderStatus.OrderStatusId);


            User buyer = await _userHelper.GetUserAsync(order.Buyer);

            OrderViewModel orderv = new OrderViewModel();
            Contact suppliercontact = await _context.Contacts.FindAsync(order.SupplierContactId);
            orderv.OrderId = order.OrderId;
            orderv.Active = order.Active;
            orderv.SupplierContactId = order.SupplierContactId;
            orderv.SellerName = $"{suppliercontact.Name} {suppliercontact.LastName}";
            orderv.Comments = order.Comments;
            orderv.CreatedBy = order.CreatedBy;
            orderv.Currency = order.Currency;
            orderv.Supplier = order.Supplier;
            orderv.SupplierQuote = order.SupplierQuote;
            orderv.EstimatedDeliveryDate = order.EstimatedDeliveryDate;
            orderv.DeliveryDate = order.DeliveryDate;
            orderv.Buyer = order.Buyer;
            orderv.BuyerName = buyer.FullName;
            orderv.OrderName = order.OrderName;
            orderv.OrderStatus = orderstatus;
            orderv.WorkOrderId = workorderid;
            orderv.WorkOrderName = workordername;
            orderv.OrderStatusId = order.OrderStatus.OrderStatusId;
            orderv.Tax = order.Tax;


            return View(orderv);

        }

        [HttpGet]
        public async Task<JsonResult> GetOrderDetails(int id)
        {

            var order = await _context.Orders
                .Include(o => o.OrderDetails).ThenInclude(d => d.Unit)
                .Include(o => o.OrderDetails).ThenInclude(d => d.Material)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return Json(new { });
            }

            User buyer = await _userHelper.GetUserAsync(order.Buyer);

            QuoteViewModel quotev = new QuoteViewModel();


            List<AllOrderDetails> details = new List<AllOrderDetails>().ToList();
            if (order.OrderDetails != null)
            {

                foreach (var detail in order.OrderDetails)
                {

                    List<Archive> archives = _context.Archives.Where(a => a.Entity == "OrderDetail" && a.EntityId == detail.OrderDetailId).ToList();

                    string archiveschain = "";

                    foreach (var item in archives)
                    {
                        archiveschain += item.ArchiveGuid.ToString() + "," + item.ArchiveName + "," + item.ArchiveId + "|";
                    }
                    if (archiveschain != "")
                    {
                        archiveschain = archiveschain.Substring(0, archiveschain.Length - 1);
                    };


                    AllOrderDetails detailsv = new()
                    {
                        OrderDetailId = detail.OrderDetailId,
                        Order = detail.Order,
                        Description = detail.Description,
                        Material = detail.Material,
                        MaterialName = detail.Material.MaterialName,
                        Price = detail.Price,
                        Quantity = detail.Quantity,
                        Unit = detail.Unit,
                        UnitName = detail.Unit.UnitName,
                        Archives = archives,
                        Received = detail.Received,
                        ArchivesChain = archiveschain

                    };


                    details.Add(detailsv);
                }

            }


            return Json(new { data = details });
        }

    

        [HttpGet]
        public async Task<JsonResult> GetOrderDetailsReceive(int id)
        {

            var order = await _context.Orders
                .Include(o => o.OrderDetails).ThenInclude(d => d.Unit)
                .Include(o => o.OrderDetails).ThenInclude(d => d.Material)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return Json(new { });
            }

            User buyer = await _userHelper.GetUserAsync(order.Buyer);


            List<AllOrderDetails> details = new List<AllOrderDetails>().ToList();
            if (order.OrderDetails != null)
            {

                foreach (var detail in order.OrderDetails)
                {

                    List<Archive> archives = _context.Archives.Where(a => a.Entity == "OrderDetail" && a.EntityId == detail.OrderDetailId).ToList();

                    string archiveschain = "";

                    foreach (var item in archives)
                    {
                        archiveschain += item.ArchiveGuid.ToString() + "," + item.ArchiveName + "," + item.ArchiveId + "|";
                    }
                    if (archiveschain != "")
                    {
                        archiveschain = archiveschain.Substring(0, archiveschain.Length - 1);
                    };


                    AllOrderDetails detailsv = new()
                    {
                        OrderDetailId = detail.OrderDetailId,
                        Order = detail.Order,
                        Description = detail.Description,
                        Material = detail.Material,
                        MaterialName = detail.Material.MaterialName,
                        Price = detail.Price,
                        Quantity = detail.Quantity,
                        Unit = detail.Unit,
                        UnitName = detail.Unit.UnitName,
                        Archives = archives,
                        ArchivesChain = archiveschain,
                        Received = detail.Received

                    };


                    details.Add(detailsv);
                }

            }


            return Json(new { data = details });
        }

        public async Task<IActionResult> GetWorkOrders()
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

            return View(lworkOrders.OrderByDescending(o => o.WorkOrderId));


        }

        [HttpPost]

        public async Task<JsonResult> LinkOT(int orderid, int workorderid) {

            string workordername = "";

            Order order = await _context.Orders
              .Include(o => o.Supplier)
              .Include(o => o.OrderDetails)
              .Include(o => o.OrderStatus)
              .Include(o => o.Currency)
              .Include(o => o.WorkOrder)
              .FirstOrDefaultAsync(m => m.OrderId == orderid);

            
            if (order.WorkOrder != null)
            {
                workorderid = order.WorkOrder.WorkOrderId;
                workordername = order.WorkOrder.WorkOrderName;
            }

            order.WorkOrder = await _context.WorkOrders.FindAsync(workorderid);
            _context.Update(order);
            await _context.SaveChangesAsync();

            ViewBag.DetailsCount = order.OrderDetails.Count();


            List<SelectListItem> orderstatus = (List<SelectListItem>)await _comboHelper.GetComboQuoteStatus(order.OrderStatus.OrderStatusId);


            User buyer = await _userHelper.GetUserAsync(order.Buyer);

            OrderViewModel orderv = new OrderViewModel();
            Contact suppliercontact = await _context.Contacts.FindAsync(order.SupplierContactId);
            orderv.OrderId = order.OrderId;
            orderv.Active = order.Active;
            orderv.SupplierContactId = order.SupplierContactId;
            orderv.SellerName = $"{suppliercontact.Name} {suppliercontact.LastName}";
            orderv.Comments = order.Comments;
            orderv.CreatedBy = order.CreatedBy;
            orderv.Currency = order.Currency;
            orderv.Supplier = order.Supplier;
            orderv.SupplierQuote = order.SupplierQuote;
            orderv.EstimatedDeliveryDate = order.EstimatedDeliveryDate;
            orderv.DeliveryDate = order.DeliveryDate;
            orderv.Buyer = order.Buyer;
            orderv.BuyerName = buyer.FullName;
            orderv.OrderName = order.OrderName;
            orderv.OrderStatus = orderstatus;
            orderv.WorkOrderId = workorderid;
            orderv.WorkOrderName = workordername;
            orderv.OrderStatusId = order.OrderStatus.OrderStatusId;
            orderv.Tax = order.Tax;


            return Json(new { success = true});
        }

        [HttpGet]
        public async Task<IActionResult> UnlinkOT(int id)
        {
            Order o = await _context.Orders.FindAsync(id);
 
            return View(o);
        }


        [HttpPost]
        public async Task<IActionResult> UnlinkOT(Order model)
        {
            Order order = await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Supplier)
                .Include(o => o.OrderStatus)
                .Include(o => o.WorkOrder)
                .Include(o => o.Currency)
                .FirstOrDefaultAsync(o => o.OrderId == model.OrderId);
            order.WorkOrder = null;

            _context.Update(order);
            await _context.SaveChangesAsync();
            TempData["EditOrderResult"] = "true";
            TempData["EditOrderMessage"] = "La OC fue desenlazada";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AddOrEditDetail(int id = 0, int detailid = 0)
        {


            //Buscamos la cotización para pasar todos sus valores al Detalle

            OrderDetail od = new OrderDetail();

            od.Order = await _context.Orders.FindAsync(id);


            //El Detalle siempre traerá un id de cotización ya que sea nuevo el detalle o actualización la Quote ya existe
            OrderDetailViewModel odv = new OrderDetailViewModel();


            //Pasamos el listado de unidades para que aparezca en un combo despues asignaremos un default si el 
            //movimiento es un Edit si es un nuevo detalle entonces se quedeará como esta sin asignar el id de la unidad


            if (detailid == 0)
            {
                List<SelectListItem> Unit = (List<SelectListItem>)await _comboHelper.GetComboUnitAsync();
                List<SelectListItem> Category = (List<SelectListItem>)await _comboHelper.GetComboCategoriesAsync();
                List<SelectListItem> MaterialType = (List<SelectListItem>)await _comboHelper.GetComboMaterialTypesAsync(0);
                List<SelectListItem> Material = (List<SelectListItem>)await _comboHelper.GetComboMaterialsAsync(0);
                odv = new()
                {
                    OrderId = od.Order.OrderId,
                    Category = Category,
                    MaterialType = MaterialType,
                    Unit = Unit,

                };
                return View(odv);
            }
            else
            {
                od = await _context.OrderDetails
                           .Include(o => o.Order)
                           .Include(o => o.Material).ThenInclude(m => m.MaterialType).ThenInclude(m => m.Category)
                           .Include(o => o.Unit)
                           .FirstOrDefaultAsync(o => o.OrderDetailId == detailid);

                List<SelectListItem> Unit = (List<SelectListItem>)await _comboHelper.GetComboUnitAsync();
                List<SelectListItem> Category = (List<SelectListItem>)await _comboHelper.GetComboCategoriesAsync();
                List<SelectListItem> MaterialType = (List<SelectListItem>)await _comboHelper.GetComboMaterialTypesAsync(od.Material.MaterialType.Category.CategoryId);
                List<SelectListItem> Material = (List<SelectListItem>)await _comboHelper.GetComboMaterialsAsync(od.Material.MaterialType.MaterialTypeId);

                odv = new()
                {
                    OrderId = od.Order.OrderId,
                    OrderDetailId = od.OrderDetailId,
                    Category = Category,
                    CategoryId = od.Material.MaterialType.Category.CategoryId,
                    MaterialTypeId = od.Material.MaterialType.MaterialTypeId,
                    MaterialType = MaterialType,
                    MaterialId = od.Material.MaterialId,
                    Material = Material,
                    Quantity = od.Quantity,
                    Price = od.Price,
                    Unit = Unit,
                    UnitId = od.Unit.UnitId,
                    Description = od.Description
                };


            };

            return View(odv);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrEditDetail(OrderDetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.OrderDetailId == 0)
                {
                    try
                    {
                        OrderDetail od = new OrderDetail();
                        od.Order = await _context.Orders.FindAsync(model.OrderId); ;
                        od.Description = model.Description;
                        od.Material = await _context.Materials.FindAsync(model.MaterialId);
                        od.Price = model.Price;
                        od.Quantity = model.Quantity;
                        od.Unit = await _context.Units.FindAsync(model.UnitId);

                        _context.Add(od);

                        Order o = await _context.Orders.FindAsync(od.Order.OrderId);
                        o.OrderStatus = await _context.OrderStatus.FindAsync(2);
                        _context.Update(o);

                        await _context.SaveChangesAsync();


                        TempData["AddOrEditOrderDetailResult"] = "true";
                        TempData["AddOrEditOrderDetailMessage"] = "La partida fué creada";
                        return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllOrderDetails", _context.OrderDetails.Where(q => q.Order.OrderId == model.OrderId).ToList()) });

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
                        OrderDetail od = await _context.OrderDetails.FindAsync(model.OrderDetailId);

                        od.Order = await _context.Orders.FindAsync(model.OrderId); ;
                        od.Description = model.Description;
                        od.Material = await _context.Materials.FindAsync(model.MaterialId);
                        od.Price = model.Price;
                        od.Quantity = model.Quantity;
                        od.Unit = await _context.Units.FindAsync(model.UnitId);
                        if (model.OrderDetailId != 0)
                        {
                            _context.Update(od);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            _context.Add(od);
                            await _context.SaveChangesAsync();
                        }

                        /*
                          Nos traemos todos los detalles

                         */
                        List<OrderDetail> list = new List<OrderDetail>();
                        list = _context.Orders.FirstOrDefault(x => x.OrderId == model.OrderId).OrderDetails.ToList();
                        List<AllOrderDetails> details = new List<AllOrderDetails>().ToList();

                        foreach (var detail in list)
                        {
                            AllOrderDetails detailsv = new()
                            {
                                OrderDetailId = detail.OrderDetailId,
                                Order = detail.Order,
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


                        return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllOrderDetails", details) });
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

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, int statusid)
        {
            Order order = await _context.Orders
                 .Include(o => o.Supplier)
                 .Include(o => o.OrderStatus)
                 .FirstOrDefaultAsync(o => o.OrderId == id);
            try
            {
                order.OrderStatus = await _context.OrderStatus.FindAsync(statusid);
                _context.Update(order);
                await _context.SaveChangesAsync();


                TempData["ChangeStatusResult"] = "true";
                TempData["ChangeStatustMessage"] = "El estatus fué actualizado";

                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllOrderDetails", _context.OrderDetails.Where(o => o.Order.OrderId == order.OrderId).ToList()) });

            }
            catch (DbUpdateException dbUpdateException)
            {

                ModelState.AddModelError(string.Empty, dbUpdateException.Message);

                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllOrderDetails", _context.OrderDetails.Where(o => o.Order.OrderId == order.OrderId).ToList()) });

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

        public async Task<IActionResult> EditOrder(int id)
        {


            Order order = await _context.Orders
                .Include(o => o.Supplier)
                .Include(o => o.OrderStatus)
                .Include(o => o.Currency)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            //-----------------------


            List<SelectListItem> buyerlist = (List<SelectListItem>)await _userHelper.GetSellersAsync();
            List<SelectListItem> users = new List<SelectListItem>();
            List<SelectListItem> buyers = new List<SelectListItem>();
            List<SelectListItem> orderstatus = new List<SelectListItem>();


            users = _context.Users
                   .Where(u => u.EmailConfirmed == true)
                   .Select(u => new SelectListItem
                   {
                       Text = u.FirstName + " " + u.LastName,
                       Value = u.UserName
                   })
                  .OrderBy(c => c.Text)
                  .ToList();

            buyers = (List<SelectListItem>)(from u in users join s in buyerlist on u.Value equals s.Value select u).ToList();
    


            List<SelectListItem> suppliers = (List<SelectListItem>)await _comboHelper.GetComboSuppliersAsync();
            List<SelectListItem> suppliercontacts = (List<SelectListItem>)await _comboHelper.GetComboContactSuppliersAsync(order.Supplier.SupplierId);
            List<SelectListItem> currencies = (List<SelectListItem>)await _comboHelper.GetComboCurrenciesAsync();

            orderstatus = _context.OrderStatus
                .Select(o => new SelectListItem
                {
                    Text = o.OrderStatusName,
                    Value = o.OrderStatusId.ToString()
                }).OrderBy(c => c.Value).ToList();


            EditOrder editorder = new EditOrder()
            {
                OrderId = order.OrderId,
                OrderName = order.OrderName,
                OrderDate = order.OrderDate,
                EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                DeliveryDate = order.DeliveryDate,
                Suppliers = suppliers,
                SupplierId = order.Supplier.SupplierId,
                SupllierContacts = suppliercontacts,
                SupplierContactId = order.SupplierContactId,
                SupplierQuote = order.SupplierQuote,
                Buyers = buyers,
                Tax = order.Tax,
                Active = order.Active,
                OrderStatus = orderstatus,
                OrderStatusId = order.OrderStatus.OrderStatusId,
                Comments = order.Comments,
                CurrencyId = order.Currency.CurrencyId,
                Currency = currencies

            };

            return View(editorder);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrder(EditOrder model)
        {
            Order order = await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Supplier)
                .Include(o => o.OrderStatus)
                .Include(o => o.Currency)
                .FirstOrDefaultAsync(o => o.OrderId == model.OrderId);

     
            if (ModelState.IsValid)
            {

                try
                {
                    Supplier supplier = await _context.Suppliers.FindAsync(model.SupplierId);

                    OrderStatus orderstatus = _context.OrderStatus.Find(model.OrderStatusId);//Estatus 2 es en modificación

                    order.Active = model.Active;
                    order.SupplierContactId = model.SupplierContactId;
                    order.Comments = model.Comments;
                    order.Supplier = supplier;
                    order.SupplierQuote = model.SupplierQuote;
                    order.OrderDate = model.OrderDate;
                    order.Buyer = model.BuyerId;
                    order.OrderName = model.OrderName;
                    order.Tax = model.Tax;
                    order.EstimatedDeliveryDate = model.EstimatedDeliveryDate;
                    order.Currency = await _context.Currencies.FindAsync(model.CurrencyId);
                    order.OrderStatus = orderstatus;
                   
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                    TempData["EditOrderResult"] = "true";
                    TempData["EditOrderMessage"] = "La OC fué actualizada";
                    List<Order> lstOrders = _context.Orders
                        .Include(l => l.OrderStatus)
                        .ToList();

                    lstOrders = lstOrders.Where(l => l.OrderStatus.OrderStatusId == 1 ||  l.OrderStatus.OrderStatusId == 2 || l.OrderStatus.OrderStatusId == 3 || l.OrderStatus.OrderStatusId == 4).ToList();
                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllOrders", lstOrders) });

                }
                catch (DbUpdateException dbUpdateException)
                {


                    ModelState.AddModelError(string.Empty, dbUpdateException.Message);
                    return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditOrder", model) });

                }

            }
            else
            {

                List<SelectListItem> buyerlist = (List<SelectListItem>)await _userHelper.GetSellersAsync();
                List<SelectListItem> users = new List<SelectListItem>();
                List<SelectListItem> buyers = new List<SelectListItem>();
                List<SelectListItem> orderstatus = new List<SelectListItem>();


                users = _context.Users
                       .Where(u => u.EmailConfirmed == true)
                       .Select(u => new SelectListItem
                       {
                           Text = u.FirstName + " " + u.LastName,
                           Value = u.UserName
                       })
                      .OrderBy(c => c.Text)
                      .ToList();

                buyers = (List<SelectListItem>)(from u in users join s in buyerlist on u.Value equals s.Value select u).ToList();

                List<SelectListItem> suppliers = (List<SelectListItem>)await _comboHelper.GetComboSuppliersAsync();
                List<SelectListItem> suppliercontacts = (List<SelectListItem>)await _comboHelper.GetComboContactSuppliersAsync(order.Supplier.SupplierId);
                List<SelectListItem> currencies = (List<SelectListItem>)await _comboHelper.GetComboCurrenciesAsync();




                orderstatus = _context.OrderStatus
                    .Select(o => new SelectListItem
                    {
                        Text = o.OrderStatusName,
                        Value = o.OrderStatusId.ToString()
                    }).OrderBy(o => o.Value).ToList();


                EditOrder editorder = new EditOrder()
                {
                    OrderId = order.OrderId,
                    OrderName = order.OrderName,
                    OrderDate = order.OrderDate,
                    DeliveryDate = order.DeliveryDate,
                    EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                    Suppliers = suppliers,
                    SupplierId = order.Supplier.SupplierId,
                    SupllierContacts = suppliercontacts,
                    SupplierContactId = order.SupplierContactId,
                    SupplierQuote = order.SupplierQuote,
                    Buyers = buyers,
                    Tax = order.Tax,
                    Active = order.Active,
                    OrderStatus = orderstatus,
                    OrderStatusId = order.OrderStatus.OrderStatusId,
                    Comments = order.Comments,
                    CurrencyId = order.Currency.CurrencyId,
                    Currency = currencies

                };

                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditOrder", editorder) });
            }

        }

        [HttpGet]
        public async Task<IActionResult> DeleteOrderDetail(int id)
        {
            OrderDetail od = await _context.OrderDetails
               .Include(d => d.Order)
               .FirstOrDefaultAsync(d => d.OrderDetailId == id);
            int quoteId = od.Order.OrderId;
            return View(od);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrderDetail(OrderDetail model)
        {
            try
            {
                //Borrar archivos

                int orderid = model.Order.OrderId;
                List<OrderDetailComment> lodc = new List<OrderDetailComment>();
                lodc = _context.OrderDetailComments.Where(c => c.OrderDetail.OrderDetailId == model.OrderDetailId).ToList();

                List<Archive> archives = _context.Archives.Where(a => a.Entity == "OrderDetail" && a.EntityId == model.OrderDetailId).ToList();

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

                

                foreach (var comment in lodc)
                {
                    _context.Remove(comment);
                }
                await _context.SaveChangesAsync();

                _context.OrderDetails.Remove(model);
                await _context.SaveChangesAsync();

                TempData["DeleteOrderDetailtResult"] = "true";
                TempData["DeleteOrderDetailMessage"] = "La partida fue eliminada";
            }

            catch
            {
                TempData["DeleteOrderDetailtResult"] = "false";
                TempData["DeleteOrderDetailMessage"] = "La partida no pudo ser eliminada";
                return RedirectToAction(nameof(Details), new { id = model.Order.OrderId });

            }

            return RedirectToAction(nameof(Details), new { id = model.Order.OrderId });

        }

        public ActionResult AddOrEditComment(int id)
        {
            List<OrderCommentView> comments = _context.OrderComments
                .Include(o => o.User)
                .Where(o => o.Order.OrderId == id)
                .Select(o => new OrderCommentView
                {
                    UserName = o.User.Email,
                    CommentId = o.CommentId,
                    OrderId = o.Order.OrderId,
                    Comment = o.Comment,
                    Usuario = o.User.FullName,
                    DateComment = o.DateComment
                })
                .OrderBy(o => o.OrderId)
                .ToList();

            ViewBag.OrderId = id;
            var name = User.Identity.Name;
            User usr = _context.Users.Where(u => u.UserName == name).FirstOrDefault();
            ViewBag.UserName = name;
            ViewBag.Usuario = usr.FullName;
            //ViewBag.UserName = "@" + name.Substring(0, name.IndexOf("@"));
            return View(comments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEditComment(int OrderId, OrderCommentView model)
        {
            if (ModelState.IsValid)
            {

                try
                {

                    OrderComment comment = new OrderComment()
                    {
                        Order = await _context.Orders.FindAsync(OrderId),
                        Comment = model.Comment,
                        DateComment = DateTime.Now,
                        User = await _userHelper.GetUserAsync(User.Identity.Name)
                    };

                    _context.Add(comment);
                    await _context.SaveChangesAsync();
                    TempData["AddOrEditCommentResult"] = "true";
                    TempData["AddOrEditCommentMessage"] = "El comentario fué agregado";

                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllOrders", _context.Quotes.ToList()) });

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
            OrderComment comment = await _context.OrderComments.FindAsync(id);
            try
            {
                _context.OrderComments.Remove(comment);
                await _context.SaveChangesAsync();
                TempData["DeleteCommentResult"] = "true";
                TempData["DeleteCommentMessage"] = "El comentario fué eliminado";
                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllOrders", _context.Quotes.ToList()) });


            }
            catch (Exception e)
            {
                TempData["DeleteCommentResult"] = "false";
                TempData["EeleteCommentMessage"] = "El comentario no pudo ser eliminado error: " + e.Message;
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllOrders", _context.Quotes.ToList()) });

            }


        }

        public ActionResult AddOrEditDetailComment(int id)
        {
            List<OrderDetailCommentView> comments = _context.OrderDetailComments
                .Include(c => c.User)
                .Where(c => c.OrderDetail.OrderDetailId == id)
                .Select(c => new OrderDetailCommentView
                {
                    UserName = c.User.Email,
                    CommentId = c.CommentId,
                    OrderDetailId = c.OrderDetail.OrderDetailId,
                    Comment = c.Comment,
                    Usuario = c.User.FullName,
                    DateComment = c.DateComment
                })
                .OrderBy(c => c.OrderDetailId)
                .ToList();

            ViewBag.OrderDetailId = id;
            var name = User.Identity.Name;
            User usr = _context.Users.Where(u => u.UserName == name).FirstOrDefault();
            ViewBag.UserName = name;
            ViewBag.Usuario = usr.FullName;

            return View(comments);


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEditDetailComment(OrderDetailCommentView model)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    OrderDetail od = await _context.OrderDetails
                        .Include(o => o.Order)
                        .FirstOrDefaultAsync(o => o.OrderDetailId == model.OrderDetailId);

                    OrderDetailComment comment = new OrderDetailComment()
                    {
                        OrderDetail = await _context.OrderDetails.FindAsync(model.OrderDetailId),
                        Comment = model.Comment,
                        DateComment = DateTime.Now,
                        User = await _userHelper.GetUserAsync(User.Identity.Name)
                    };

                    _context.Add(comment);
                    await _context.SaveChangesAsync();
                    TempData["AddOrEditDetailCommentResult"] = "true";
                    TempData["AddOrEditDetailCommentMessage"] = "El comentario fué agregado";

                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllOrderDetails", _context.OrderDetails.Where(o => o.Order.OrderId == od.Order.OrderId).ToList()) });

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
            OrderDetailComment comment = await _context.OrderDetailComments.FindAsync(id);
            try
            {
                _context.OrderDetailComments.Remove(comment);
                await _context.SaveChangesAsync();
                TempData["DeleteDetailCommentResult"] = "true";
                TempData["DeleteDetailCommentMessage"] = "El comentario fué eliminado";
                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllOrderDetails", _context.Orders.ToList()) });


            }
            catch (Exception e)
            {
                TempData["DeleteDetailCommentResult"] = "false";
                TempData["EeleteDetailCommentMessage"] = "El comentario no pudo ser eliminado error: " + e.Message;
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllOrderDetails", _context.Orders.ToList()) });

            }


        }


        public async Task<FileResult> PrintReport(int OrderId)
        {

            Order order = await _context.Orders.FindAsync(OrderId);

            Stream stream = new MemoryStream(await _reportHelper.GenerateOrderReportPDFAsync(OrderId));

            return File(stream, "application/pdf", $"{order.OrderName}{".pdf"}");
        }

        [HttpGet]
        public async Task<IActionResult> CopyOrder(int id)
        {
            Order order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
            CopyOrder corder = new CopyOrder();
            corder.OrderId = order.OrderId;
            corder.OrderName = order.OrderName;
            corder.copyfilesdetails = false;
            corder.copyfilesheader = false;
            return View(corder);
        }

        [HttpPost]
        public async Task<IActionResult> CopyOrder(CopyOrder model)
        {

            ////Orden de la que se basa la nueva
            ///
            Order oldorder = await _context.Orders
                .Include(o => o.OrderDetails).ThenInclude(o => o.Material)
                .Include(o => o.OrderDetails).ThenInclude(o => o.Unit)
                .Include(o => o.Supplier)
                .Include(o => o.Currency)
                .FirstOrDefaultAsync( o => o.OrderId == model.OrderId );    

   


            //Formamos el nombre

            string ordername = DateTime.Now.ToString("yyyyMMdd");
            string Lastnumber = ""; //Ultimo numero consecutivo de la cotización
            string strnumber = "";
            int Consec = 0;

            ///Por default dejamos 10 dias de vigencia
            TimeSpan validuntildate = new TimeSpan(10, 0, 0, 0); //Diez dias de vigencia por defecto

            ordername = "OC-" + ordername;

            // -------------------------

            Order Lastorder = await _context.Orders.Where(o => o.OrderName.Substring(0, 11) == ordername.Substring(0, 11)).OrderBy(o => o.OrderId).LastOrDefaultAsync();//Ultima cotizacion


            if (Lastorder != null)
            {
                Lastnumber = Lastorder.OrderName.Substring(12, 3);
                Consec = Int32.Parse(Lastnumber);
            }
            else
            {
                Lastnumber = "000";
                Consec = Int32.Parse(Lastnumber);
            }

            Consec += 1;

            strnumber = $"000{Consec}";

            ordername = ordername + "-" + strnumber.Substring(strnumber.Length - 3, 3);

            //-----------------------

            User createdBy = await _userHelper.GetUserAsync(User.Identity.Name);
            OrderStatus orderstatus = await _context.OrderStatus.FindAsync(2);//Como es copia nace en modificación
            QuoteStatus quotestatus = await _context.QuoteStatus.FindAsync(2);//Como es copia nace en modificación

            Order order = new Order() { 
                Active = true,
                Comments = oldorder.Comments,
                CreatedBy = createdBy,
                Currency = oldorder.Currency,
                Supplier = oldorder.Supplier,
                SupplierContactId = oldorder.SupplierContactId,
                SupplierQuote = oldorder.SupplierQuote,
                Buyer = oldorder.Buyer,
                OrderDate = DateTime.Now,
                OrderName = ordername,
                Tax = oldorder.Tax,
                OrderStatus = orderstatus,
                DeliveryDate = null,
                EstimatedDeliveryDate = null,
            };
            _context.Add(order);
            await _context.SaveChangesAsync();

            foreach (OrderDetail ood in oldorder.OrderDetails)
            {
                OrderDetail od = new OrderDetail() {
                    Order = order,
                    Description = ood.Description,
                    Material = ood.Material,
                    Unit = ood.Unit,
                    Price = ood.Price,
                    Quantity = ood.Quantity,

                };
                _context.Add(od);
                await _context.SaveChangesAsync();

                if (model.copyfilesdetails == true)
                {
                    List<Archive> archives = _context.Archives.Where(a => a.Entity == "OrderDetail" && a.EntityId == ood.OrderDetailId).ToList();

                    foreach (Archive arch in archives)
                    {

                        Guid archiveguid = Guid.Empty;

                        archiveguid = await _blobHelper.CopyBlobAsync(arch.ArchiveGuid, "archives");

                        Archive archive = new Archive();
                        archive.ArchiveGuid = archiveguid;
                        archive.Entity = arch.Entity;
                        archive.EntityId = od.OrderDetailId;
                        archive.ArchiveName = arch.ArchiveName;
                        _context.Add(archive);
                        await _context.SaveChangesAsync();
                    }

                }

            }


            //Si se eligio copiar archivos de cabecera los copiamos a la nueva Cotizacion

            if (model.copyfilesheader == true)
            {
                List<Archive> qarchives = _context.Archives.Where(a => a.Entity == "Order" && a.EntityId == oldorder.OrderId).ToList();

                foreach (Archive a in qarchives)
                {
                    Guid archiveguid = Guid.Empty;
                    archiveguid = await _blobHelper.CopyBlobAsync(a.ArchiveGuid, "archives");
                    Archive archive = new Archive();
                    archive.ArchiveGuid = archiveguid;
                    archive.Entity = a.Entity;
                    archive.EntityId = order.OrderId;
                    archive.ArchiveName = a.ArchiveName;
                    _context.Add(archive);
                    await _context.SaveChangesAsync();

                }

            }

            TempData["CopyOrderResult"] = "true";
            TempData["CopyOrderMessage"] = "La Orden de compra fué copiada";
            return RedirectToAction("Index", "Orders", new { id = order.OrderId });
        }

        [HttpPost]
        public async Task<IActionResult> CopyDetail(int orderDetailId)
        {

            try
            {
                OrderDetail od = await _context.OrderDetails
                .Include(o => o.Order)
                .Include(o => o.Unit)
                .Include(o => o.Material)
                .FirstOrDefaultAsync(o => o.OrderDetailId == orderDetailId);

                OrderDetail odn = new OrderDetail();
                odn.Order = od.Order;
                odn.Description = od.Description;
                odn.Quantity = od.Quantity;
                odn.Material = od.Material;
                odn.Price = od.Price;
                odn.OrderDetailComments = od.OrderDetailComments;
                odn.Unit = od.Unit;

                _context.Add(odn);
                await _context.SaveChangesAsync();

                return Json(new { data = "success", isValid = true });
            }
            catch (Exception)
            {

                return View();

            }





        }

        [HttpGet]
        public async Task<IActionResult> Receive(int? id)
        {


            ViewBag.Result = "";
            ViewBag.Message = "";


            if (TempData != null)
            {
                if (TempData["AddOrEditQuoteDetailResult"] != null)
                {

                    ViewBag.Result = TempData["AddOrEditOrderDetailResult"].ToString();
                    ViewBag.Message = TempData["AddOrEditOrderDetailMessage"].ToString();
                    TempData.Remove("AddOrEditOrderDetailResult");
                    TempData.Remove("AddOrEditOrderDetailMessage");
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
                if (TempData["DeleteOrderDetailtResult"] != null)
                {
                    ViewBag.Result = TempData["DeleteOrderDetailtResult"].ToString();
                    ViewBag.Message = TempData["DeleteOrderDetailMessage"].ToString();
                    TempData.Remove("DeleteOrderDetailtResult");
                    TempData.Remove("DeleteOrderDetailMessage");
                }


            }


            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Supplier)
                .Include(o => o.OrderDetails)
                .Include(o => o.OrderStatus)
                .Include(o => o.Currency)
                .Include(o => o.WorkOrder)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            int workorderid = 0;
            string workordername = "";

            if (order.WorkOrder != null)
            {
                workorderid = order.WorkOrder.WorkOrderId;
                workordername = order.WorkOrder.WorkOrderName;
            }

            ViewBag.DetailsCount = order.OrderDetails.Count();

            if (order == null)
            {
                return NotFound();
            }

            List<SelectListItem> orderstatus = (List<SelectListItem>)await _comboHelper.GetComboOrderStatus(order.OrderStatus.OrderStatusId);

            List<SelectListItem> receptors = (List<SelectListItem>)await _userHelper.GetUsersByRoleAsync(Enums.UserType.Receptor);


            User buyer = await _userHelper.GetUserAsync(order.Buyer);

            OrderReceiptViewModel orderv = new OrderReceiptViewModel();
            Contact suppliercontact = await _context.Contacts.FindAsync(order.SupplierContactId);
            orderv.OrderId = order.OrderId;
            orderv.Active = order.Active;
            orderv.SupplierContactId = order.SupplierContactId;
            orderv.SellerName = $"{suppliercontact.Name} {suppliercontact.LastName}";
            orderv.Comments = order.Comments;
            orderv.CreatedBy = order.CreatedBy;
            orderv.Currency = order.Currency;
            orderv.Supplier = order.Supplier;
            orderv.SupplierQuote = order.SupplierQuote;
            orderv.EstimatedDeliveryDate = order.EstimatedDeliveryDate;
            orderv.DeliveryDate = order.DeliveryDate;
            orderv.Buyer = order.Buyer;
            orderv.BuyerName = buyer.FullName;
            orderv.OrderName = order.OrderName;
            orderv.OrderStatus = orderstatus;
            orderv.WorkOrderId = workorderid;
            orderv.WorkOrderName = workordername;
            orderv.OrderStatusId = order.OrderStatus.OrderStatusId;
            orderv.Tax = order.Tax;
            orderv.Receptors = receptors;


            return View(orderv);

        }


        [HttpPost]
        public async Task<IActionResult> Receive(OrderReceiptViewModel model)
        {
            string receiptdetails = model.ReceiptDetails;
            receiptdetails = receiptdetails.Substring(0, receiptdetails.Length - 1);
            return RedirectToAction("ReceiptPreview", new { orderid = model.OrderId, receiptDetails = receiptdetails, receiptComments = model.ReceiptComments, receivedBy  = model.ReceivedBy});
        }


        [HttpGet]
        public async Task<IActionResult> ReceiptPreview(int orderid, string receiptDetails, string receiptComments, string receivedBy)
        {  //Es el id de Remision


            Order order = await _context.Orders
                .Include(o => o.Supplier).ThenInclude(s => s.City)
                .FirstOrDefaultAsync(o => o.OrderId == orderid);

            List<OrderDetail> lods = _context.OrderDetails
                                            .Include(d => d.Unit)
                                            .Include(d => d.Material)
                                            .Where(d => d.Order.OrderId == orderid).ToList();
            List<AllOrderDetails> lodresult = new List<AllOrderDetails>();

            User receptor = await _userHelper.GetUserAsync(receivedBy);



            var mtxdetails = receiptDetails.Split('|');

            foreach (OrderDetail d in lods)
            {
                foreach (var detail in mtxdetails)
                {
                    var detailid = detail.Split(",");
                    if (d.OrderDetailId.ToString() == detailid[0].ToString())
                    {
                        AllOrderDetails orderdetail = new AllOrderDetails();
                        orderdetail.OrderDetailId = d.OrderDetailId;
                        orderdetail.Unit = d.Unit;
                        orderdetail.Price = d.Price;
                        orderdetail.Quantity = Decimal.Parse(detailid[1]);
                        orderdetail.Material = d.Material;
                        orderdetail.Description = d.Description;
                        orderdetail.Received = d.Received;
                        lodresult.Add(orderdetail);
                        break;

                    }
                }
            }
           

            OrderReceiptViewModel orvm = new OrderReceiptViewModel();

            String orderreceiptname = DateTime.Now.ToString("yyyyMMdd"); //Variable formadora de nombre de coti
            orderreceiptname = "REC-" + orderreceiptname + "-000";

           
            Contact Seller = await _context.Contacts.FindAsync(order.SupplierContactId);

            User Buyer = await _userHelper.GetUserAsync(order.Buyer);

            if (receiptComments == null || receiptComments == "")
            {
                receiptComments = "NA";
            }

            orvm.OrderId = orderid;
            orvm.OrderDetails = lodresult;
            orvm.Supplier = order.Supplier;
            orvm.BuyerName = $"{Buyer.FirstName} {Buyer.LastName}";
            orvm.SellerName = $"{Seller.Name} {Seller.LastName}";
            orvm.ReceiptComments = receiptComments;
            orvm.OrderReceiptName = orderreceiptname;
            orvm.ReceiptDetails = receiptDetails;
            orvm.ReceivedBy = receivedBy;
            orvm.ReceptorName = $"{receptor.FirstName} {receptor.LastName}";





            return View("ReceiptPreview", orvm);
        }

        [HttpGet]
        public async Task<IActionResult> ReceiptConfirm(int orderid, string receiptDetails, string receiptComments, string receivedBy)
        {  //Es el id de Remision
            Order o = await _context.Orders.FindAsync(orderid);
            OrderReceiptViewModel ormvm = new OrderReceiptViewModel();
            ormvm.OrderId = orderid;
            ormvm.ReceiptDetails = receiptDetails;
            ormvm.ReceiptComments = receiptComments;
            ormvm.ReceivedBy = receivedBy;
            return View(ormvm);
        }
        [HttpPost]
        public async Task<IActionResult> ReceiptConfirm(OrderReceiptViewModel model)
        {
            String receiptname = DateTime.Now.ToString("yyyyMMdd"); //Variable formadora de nombre de coti
            String Lastnumber = ""; //Ultimo numero consecutivo de la cotización
            String strnumber = "";
            int Consec = 0;


            receiptname = "REC-" + receiptname;

            // -------------------------

           OrderReceipt LastOR = await _context.OrderReceipts.Where(o => o.ReceiptName.Substring(0, 12) == receiptname).OrderBy(o => o.OrderReceiptId).LastOrDefaultAsync();//Ultima cotizacion

            if (LastOR != null)
            {
                Lastnumber = LastOR.ReceiptName.Substring(13, 3);
                Consec = Int32.Parse(Lastnumber);
            }
            else
            {
                Lastnumber = "000";
                Consec = Int32.Parse(Lastnumber);
            }

            Consec += 1;

            strnumber = $"000{Consec}";

            Order order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == model.OrderId);

            receiptname = receiptname + "-" + strnumber.Substring(strnumber.Length - 3, 3);
            User receivedBy = await _userHelper.GetUserAsync(model.ReceivedBy);

            OrderReceipt oR = new OrderReceipt();
            oR.Order = order;
            oR.ReceiptName = receiptname;
            oR.ReceiptDate = DateTime.Now;
            oR.Comments = model.ReceiptComments;
            oR.ReceivedBy = receivedBy;
            _context.Add(oR);
            await _context.SaveChangesAsync();
       

            var mtxdetails = model.ReceiptDetails.Split('|');

            for (int i = 0; i < mtxdetails.Length; i++)
            {
                var detail = mtxdetails[i].Split(",");
                OrderReceiptDetail odr = new OrderReceiptDetail();
                OrderDetail orderdetail = await _context.OrderDetails.FindAsync(Convert.ToInt32(detail[0]));
                odr.OrderReceipt = oR;
                odr.OrderDetail = orderdetail;
                odr.Quantity = Convert.ToDecimal(detail[1]);
                _context.Add(odr);
                await _context.SaveChangesAsync();

                //vamos a dejar que sea negativo se remisionamos mas piezas de las que estan declaradas en Quantity
                //Entonces actualizamos las embarcadas ya que si estamos remisionando quiere decir que estamos embarcando
                orderdetail.Received = orderdetail.Received + Convert.ToDecimal(detail[1]);
                _context.Update(orderdetail);
                await _context.SaveChangesAsync();


            }


            TempData["ReceiptResult"] = "true";
            TempData["ReceiptMessage"] = $"El recibo {receiptname} fué creado";

            OrderReceiptViewModel orvm = new OrderReceiptViewModel();

            orvm.OrderId = order.OrderId;

            return RedirectToAction(nameof(Details), new { id = model.OrderId });
        }

        public async Task<FileResult> PrintReceipt(string receiptname) ///usuamos el nombre ya que este lo obtenemos del mensaje al crear la remision y viene en el TempData["RemisionMessage"]
        {

            OrderReceipt receipt = _context.OrderReceipts.Where(r => r.ReceiptName == receiptname).FirstOrDefault();

            Stream stream = new MemoryStream(await _reportHelper.GenerateReceiptReportPDFAsync(receipt.OrderReceiptId));

            return File(stream, "application/pdf", $"{receipt.ReceiptName}{".pdf"}");
        }

    }
}
