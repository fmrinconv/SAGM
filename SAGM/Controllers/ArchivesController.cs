using Azure.Storage.Blobs;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAGM.Data;
using SAGM.Data.Entities;
using SAGM.Helpers;
using SAGM.Models;
using System.IO;
using ClosedXML.Excel;

using static SAGM.Helpers.ModalHelper;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel;
using System.Globalization;
using static SkiaSharp.HarfBuzz.SKShaper;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace SAGM.Controllers
{

    [Authorize(Roles = "Administrador,Usuario,Comprador,Vendedor")]
    public class ArchivesController : Controller
    {
        private readonly SAGMContext _context;
        private readonly IBlobHelper _blobHelper;
        private readonly IUserHelper _userHelper;
        private readonly IComboHelper _comboHelper;
        private readonly IConfiguration _configuration;


        public ArchivesController(SAGMContext context, IBlobHelper blobHelper, IUserHelper userHelper, IComboHelper comboHelper, IConfiguration configuration)
        {
            _context = context;
            _blobHelper = blobHelper;
            _userHelper = userHelper;
            _comboHelper = comboHelper;
            _configuration = configuration;
        }


        //Este proceso es exclusivo para Finanzas y sirve para leer un excel y poder subir su info a la BD
        public IActionResult UploadFinanceFile(string entity = "InvoicesTralix", int entityid = 0)
        {
            AddArchiveViewModel model = new AddArchiveViewModel();
            model.Entity = entity;
            model.EntityId = entityid;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]///Este proceso es usado para almacenar archivos y dejarlo guardado ya que casi no se utilizará la funcion eliminar
        //
        public async Task<IActionResult> UploadFinanceFile(AddArchiveViewModel model)
        {
            //El siguiente proceso se ejecuta seguen el siguiente orden:
            //1) Para eficientar el proceso vamos a seleccionar solo el año actual y el año anterior seran calculados a partir de la fecha de hoy
            //2) Se sube el archivo al Blob
            //3) Se guarda el nombre del archivo en la tabla de Archives
            //4) Se lee el archivo subido con XLWorkBook y se carga en memoria
            //5) Se limpia la tabla invoicesTralixes para dejar vacio el esqueleto
            //6) Se cargan todas las facturas en el objeto lstinvoicestralix y se sube de un solo golpe todas las facturas  
            //7) Ahora se pasaran todas las facturas de InvoicesTralix a Invoices Eliminando primero las facturas que ya existen en Invoices y que son el listado cargado en InvoicesTralix (ya que pueden haber actualizaciones de estatus

            string act_year;
            string old_year;

            act_year = DateTime.Now.Year.ToString();
            old_year = Convert.ToString(DateTime.Now.Year - 1);
            Guid archiveguid = Guid.Empty;

            if (model.ArchiveFile != null)
            {
                archiveguid = await _blobHelper.UploadBlobAsync(model.ArchiveFile, "financefiles");
            }

            model.ArchiveGuid = archiveguid;

            Archive archive = new Archive();
            archive.ArchiveGuid = archiveguid;
            archive.Entity = model.Entity;
            archive.EntityId = model.EntityId;
            archive.UploadDate = DateTime.Now;
            archive.ArchiveName = model.ArchiveFile.FileName;
            _context.Add(archive);
            await _context.SaveChangesAsync();


            string connectionstring = _configuration["Blob:ConnectionString"];
            BlobClient blobClient = new BlobClient(connectionstring, "financefiles", archiveguid.ToString());
            using (var stream = new MemoryStream())
            {
                blobClient.DownloadTo(stream);
                stream.Position = 0;
                var contentType = (blobClient.GetProperties()).Value.ContentType;
                

                var wb = new XLWorkbook(stream);
                var ws = wb.Worksheets.First();

                var range = ws.RangeUsed();
                var colCount = range.ColumnCount();
                var rowCount = range.RowCount();

                var i = 2;


                var result = await  _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE invoicesTralixes");

                List<InvoicesTralix> lstinvoicestralix = new List<InvoicesTralix>();

                while (i < rowCount + 1)
                {
                    if (ws.Cell(i, 4).Value.ToString().Contains(old_year) || ws.Cell(i, 4).Value.ToString().Contains(act_year))
                    {
                        InvoicesTralix invoiceTralix = new InvoicesTralix();

                        invoiceTralix.Serie = ws.Cell(i, 1).Value.ToString();
                        invoiceTralix.Folio = ws.Cell(i, 2).Value.ToString();
                        invoiceTralix.UUID = ws.Cell(i, 3).Value.ToString();
                        invoiceTralix.FechadeEmision = ws.Cell(i, 4).Value.ToString();
                        invoiceTralix.RFCEmisor = ws.Cell(i, 5).Value.ToString();
                        invoiceTralix.NombredelEmisor = ws.Cell(i, 6).Value.ToString();
                        invoiceTralix.RFCReceptor = ws.Cell(i, 7).Value.ToString();
                        invoiceTralix.NombredelReceptor = ws.Cell(i, 8).Value.ToString();
                        invoiceTralix.Subtotal = ws.Cell(i, 9).Value.ToString();
                        invoiceTralix.IVATrasladado = ws.Cell(i, 10).Value.ToString();
                        invoiceTralix.Total = ws.Cell(i, 11).Value.ToString();
                        invoiceTralix.Moneda = ws.Cell(i, 12).Value.ToString();
                        invoiceTralix.EstadoFiscal = ws.Cell(i, 13).Value.ToString();
                        invoiceTralix.Pagado = ws.Cell(i, 14).Value.ToString();
                        invoiceTralix.TipodeComprobante = ws.Cell(i, 15).Value.ToString();
                        invoiceTralix.TipodeCFDI = ws.Cell(i, 16).Value.ToString();
                        invoiceTralix.FechadePago = ws.Cell(i, 17).Value.ToString();
                        invoiceTralix.ComentariodePago = ws.Cell(i, 18).Value.ToString();
                        invoiceTralix.MetododePago = ws.Cell(i, 19).Value.ToString();

                        lstinvoicestralix.Add(invoiceTralix);
                    }
                    
                    i++;
                }


                _context.invoicesTralixes.AddRange(lstinvoicestralix);
                result = await _context.SaveChangesAsync();

                /* Aqui seleccionamos todas los UUID de InvoicesTralix para que sean el WHERE de lo que vamos a Eliminar de Invoices*/
                    List<string> lstIdsInvoicesTralix = new List<string>();
                    lstIdsInvoicesTralix = await _context.invoicesTralixes.Select( c => c.UUID ).ToListAsync();
                /**-----------------------------------------------------------------------------------------------------------*/



                /*Ahora seleccionamos todos los Invoices a eliminar con el criterios de lstIdsInvoicesTralix y los eliminamos*/
                    List<Invoice> LstInvoices = _context.Invoices.Where( r=>lstIdsInvoicesTralix.Contains(r.UUID)).ToList();
                    _context.Invoices.RemoveRange(LstInvoices);
                    result = await _context.SaveChangesAsync();
                //-------------------------------------------------------------------------------------------------------------*/


                List<InvoicesTralix> InvoicesTralix = await _context.invoicesTralixes.ToListAsync();
                List<Invoice> Invoices = new List<Invoice>();

               
                foreach (var invoicetralix in InvoicesTralix)
                {
                    Invoice invoice = new Invoice();
                    invoice.Serie = invoicetralix.Serie;
                    invoice.Folio = ToNullableInt(invoicetralix.Folio);
                    invoice.UUID = invoicetralix.UUID;
                    invoice.EmisionDate = ToNullableDate(invoicetralix.FechadeEmision);
                    invoice.EmisorTaxId = invoicetralix.RFCEmisor;
                    invoice.EmisorName = invoicetralix.NombredelEmisor;
                    invoice.ReceptorTaxId = invoicetralix.RFCReceptor;
                    invoice.ReceptorName = invoicetralix.NombredelReceptor;
                    invoice.Subtotal = ToNullableDecimal(invoicetralix.Subtotal);
                    invoice.TrasladedTax = ToNullableDecimal(invoicetralix.IVATrasladado);
                    invoice.Total = ToNullableDecimal(invoicetralix.Total);
                    invoice.Currency = invoicetralix.Moneda;
                    invoice.TaxStatus = invoicetralix.EstadoFiscal;
                    invoice.PayStatus = invoicetralix.Pagado;
                    invoice.RecipeType = invoicetralix.TipodeComprobante;
                    invoice.CfdiType = invoicetralix.TipodeCFDI;
                    invoice.PayDate = ToNullableDate(invoicetralix.FechadePago);
                    invoice.payMethod = invoicetralix.MetododePago;
                    Invoices.Add(invoice);  

                   
                }

                _context.Invoices.AddRange(Invoices);
                result = await _context.SaveChangesAsync();

                result = await _context.Database.ExecuteSqlRawAsync("DELETE FROM InvoicesCompacted WHERE [Year] IN(" + act_year + ", " + old_year + ")");
                result = await _context.SaveChangesAsync();

                string command = "INSERT INTO InvoicesCompacted (Day,Month,Year,Subtotal,Total,[RFC Receptor],[Date])\r\n\t\tSELECT DAY(I.[Fecha de Emisión]) AS Dia,\r\n\t\t\t   MONTH(I.[Fecha de Emisión]) AS Mes,\r\n\t\t\t   YEAR(I.[Fecha de Emisión]) AS Año,\r\n\t\tSUM(CASE I.Moneda\r\n\t\t\tWHEN 'USD' THEN Subtotal*E.Exchangerate\r\n\t\t\tELSE Subtotal\r\n\t\tEND) AS Subtotal,\r\n\t\tSUM(CASE I.Moneda\r\n\t\t\tWHEN 'USD' THEN Total*E.Exchangerate\r\n\t\t\tELSE Total\r\n\t\tEND) As Total,\r\n\t\t[RFC Receptor], \r\n\t\t I.[Fecha de Emisión]\r\n\t\t FROM Invoices I\r\n\t\tLEFT JOIN ExchangeRates E ON (CONVERT(date,I.[Fecha de Emisión]) = E.[Date]  AND YEAR(E.[Date]) IN (" + act_year + "," + old_year + "))\r\n\t\t\tWHERE [Tipo de Comprobante] = 'FACTURA'\r\n\t\tAND [Estado Fiscal] IN ('ACTIVO','VIGENTE','VIGENTE:NO CANCELABLE', 'VIGENTE:SIN ACEPTACIÓN', 'VIGENTE:CON ACEPTACIÓN')\r\n\t\tGroup By  [RFC Receptor],I.[Fecha de Emisión]\r\n";
                result = await _context.Database.ExecuteSqlRawAsync(command);
                result = await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Archives");
            }

      
          

            return View();
        }


        //Este proceso es exclusivo para Finanzas y sirve para leer un excel y poder subir su info a la BD
        public IActionResult UploadExchangeRateFile(string entity = "ExchangeRate", int entityid = 0)
        {
            AddArchiveViewModel model = new AddArchiveViewModel();
            model.Entity = entity;
            model.EntityId = entityid;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]///Este proceso es usado para almacenar archivos y dejarlo guardado ya que casi no se utilizará la funcion eliminar
        //
        public async Task<IActionResult> UploadExchangeRateFile(AddArchiveViewModel model)
        {
            //El siguiente proceso se ejecuta seguen el siguiente orden:
            //1) Se sube el archivo al Blob
            //2) Se guarda el nombre del archivo en la tabla de Archives
            //3) Se lee el archivo subido con XLWorkBook y se carga en memoria
            //4) Se elimina de la tabla ExchangeRate solo las fechas a modificarse
            //5) Se cargan las fechas nuevas en el objeto lstExchangeRate y se sube de un solo golpe todas los registros nuevos  
            //6) Se Agregan las fechas que no existan en ExchangeRates para poder hacer en el proceso de concentrado el Join con los tipos de cambio de las fechas que no existen  esto debido a aquellas facturas que se hicieron en dian inhábil o donde no hay un puntal día con fecha de cambio 

            Guid archiveguid = Guid.Empty;

            if (model.ArchiveFile != null)
            {
                archiveguid = await _blobHelper.UploadBlobAsync(model.ArchiveFile, "financefiles");
            }

            model.ArchiveGuid = archiveguid;

            Archive archive = new Archive();
            archive.ArchiveGuid = archiveguid;
            archive.Entity = model.Entity;
            archive.EntityId = model.EntityId;
            archive.ArchiveName = model.ArchiveFile.FileName;
            _context.Add(archive);
            await _context.SaveChangesAsync();


            string connectionstring = _configuration["Blob:ConnectionString"];
            BlobClient blobClient = new BlobClient(connectionstring, "financefiles", archiveguid.ToString());
            using (var stream = new MemoryStream())
            {
                blobClient.DownloadTo(stream);
                stream.Position = 0;
                var contentType = (blobClient.GetProperties()).Value.ContentType;


                var wb = new XLWorkbook(stream);
                var ws = wb.Worksheets.First();

                var range = ws.RangeUsed();
                var colCount = range.ColumnCount();
                var rowCount = range.RowCount();

                var i = 2;


          
                List<ExchangeRate> lstexchangerates = new List<ExchangeRate>();

                List<DateOnly> lstdates = new List<DateOnly>();

             

                while (i < rowCount + 1)
                {
                    ExchangeRate exchangeRate = new ExchangeRate();

                    DateOnly date = new DateOnly();


                    DateTime datetime = new DateTime();

                    if (ws.Cell(i, 1).TryGetValue(out datetime))
                    {
                        datetime = ws.Cell(i, 1).GetDateTime();
                        date = new DateOnly(datetime.Year, datetime.Month, datetime.Day);                  
                    }



                    exchangeRate.Date = date;
                    exchangeRate.Exchangerate = Convert.ToDecimal(ws.Cell(i, 2).Value.ToString());


                    lstexchangerates.Add(exchangeRate);
                    lstdates.Add(date);
                    i++;
                }

                /*Ahora seleccionamos todos los Invoices a eliminar con el criterios de lstIdsInvoicesTralix y los eliminamos*/
                List<ExchangeRate> lstexchangeratesToDelete = _context.ExchangeRates.Where(r => lstdates.Contains(r.Date)).ToList();
                _context.ExchangeRates.RemoveRange(lstexchangeratesToDelete);
                await _context.SaveChangesAsync();
                //-------------------------------------------------------------------------------------------------------------*/


                _context.ExchangeRates.AddRange(lstexchangerates);
                await _context.SaveChangesAsync();

                string Command = "SELECT  distinct CONVERT(date,[Fecha de Emisión]) As NoDate , \r\n\t\t\t\tCONVERT(Date,GETDATE()) AS NewDate,\r\n\t\t\t\tCONVERT(decimal(18,4),0) AS ChangeRate\r\n\t\tinto #tmp\r\nFROM Invoices\r\nWHERE  CONVERT(date,[Fecha de Emisión]) NOT IN (SELECT [Date] FROM ExchangeRates)";

                Command += "UPDATE #tmp\r\nSET NewDate = (SELECT MAX([Date]) FROM ExchangeRates  WHERE [Date] < NoDate)\r\nFROM #tmp \r\n";

                Command += "UPDATE #tmp\r\nSET ChangeRate = B.ExchangeRate\r\nFROM #tmp A\r\nINNER JOIN ExchangeRates B ON (A.NewDate = B.[Date])\r\n";

                Command += "\r\nINSERT INTO ExchangeRates\r\n([Date],Exchangerate)\r\nSELECT [NoDate], changeRate\r\nFROM #tmp\r\n\r\nDROP TABLE #tmp";


                var result = await _context.Database.ExecuteSqlRawAsync(Command);
                result = await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Archives");
            }




        }


        public IActionResult AddArchive(string entity = "", int entityid=0)
        {
            AddArchiveViewModel model = new AddArchiveViewModel();
            model.Entity = entity;
            model.EntityId = entityid;
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]///Este proceso es usado para almacenar archivos y dejarlo guardado ya que casi no se utilizará la funcion eliminar
        //
        public async Task<IActionResult> AddArchive(AddArchiveViewModel model)
        {
            Guid archiveguid = Guid.Empty;

            if (model.ArchiveFile != null)
            {
                archiveguid = await _blobHelper.UploadBlobAsync(model.ArchiveFile, "archives");
            }

            model.ArchiveGuid = archiveguid;

            Archive archive = new Archive();
            archive.ArchiveGuid = archiveguid;
            archive.Entity = model.Entity;
            archive.EntityId = model.EntityId;
            archive.ArchiveName = model.ArchiveFile.FileName;
            archive.UploadDate = DateTime.Now;
            _context.Add(archive);
            await _context.SaveChangesAsync();



            switch (archive.Entity)
            {
                case "QuoteDetail":
                    TempData["AddArchiveResult"] = "true";
                    TempData["AddArchiveMessage"] = "La carga de archivos se ha completado con éxito";
                    QuoteDetail qd = await _context.QuoteDetails
                                        .Include(qd => qd.Quote)
                                        .FirstOrDefaultAsync(qd => qd.QuoteDetailId == archive.EntityId);

                    return RedirectToAction("Details", "Quotes", new { id = qd.Quote.QuoteId, quoteDetilId = qd.QuoteDetailId });

                case "Quote":
                    TempData["AddArchiveResult"] = "true";
                    TempData["AddArchiveMessage"] = "La carga de archivos se ha completado con éxito";
                    Quote q = await _context.Quotes
                                        .FirstOrDefaultAsync(q => q.QuoteId == archive.EntityId);

                    return RedirectToAction("Index", "Quotes", new { id = q.QuoteId });

                case "WorkOrder":
                    TempData["AddArchiveResult"] = "true";
                    TempData["AddArchiveMessage"] = "La carga de archivos se ha completado con éxito";
                    WorkOrder w = await _context.WorkOrders
                                        .FirstOrDefaultAsync(w => w.WorkOrderId == archive.EntityId);

                    return RedirectToAction("Index", "WorkOrders", new { id = w.WorkOrderId });

                case "WorkOrderDetail":
                    TempData["AddArchiveResult"] = "true";
                    TempData["AddArchiveMessage"] = "La carga de archivos se ha completado con éxito";
                    WorkOrderDetail wd = await _context.WorkOrderDetails
                                        .Include(wd => wd.WorkOrder)
                                        .FirstOrDefaultAsync(wd => wd.WorkOrderDetailId == archive.EntityId);

                    return RedirectToAction("Details", "WorkOrders", new { id = wd.WorkOrder.WorkOrderId, workOrderId = wd.WorkOrderDetailId });

                case "Order":
                    TempData["AddArchiveResult"] = "true";
                    TempData["AddArchiveMessage"] = "La carga de archivos se ha completado con éxito";
                    Order o = await _context.Orders
                                        .FirstOrDefaultAsync(o => o.OrderId == archive.EntityId);

                    return RedirectToAction("Index", "Orders", new { id = o.OrderId });

                case "OrderDetail":
                    TempData["AddArchiveResult"] = "true";
                    TempData["AddArchiveMessage"] = "La carga de archivos se ha completado con éxito";
                    OrderDetail od = await _context.OrderDetails
                                        .Include(od => od.Order)
                                        .FirstOrDefaultAsync(od => od.OrderDetailId == archive.EntityId);

                    return RedirectToAction("Details", "Orders", new { id = od.Order.OrderId, orderDetilId = od.OrderDetailId });

                default:
                    break;
            }

            

            return View(archive);
        }


        [NoDirectAccess]
        [HttpPost]
        public async Task<IActionResult> DeleteArchive(int id, string controller, int entityId)
        {


            Archive archive = await _context.Archives
                .FirstOrDefaultAsync(m => m.ArchiveId == id);

            try
            {
                _context.Archives.Remove(archive);
                await _context.SaveChangesAsync();
                await _blobHelper.DeleteBlobAsync(archive.ArchiveGuid, "archives");
                return Json(new { isValid = true, success = true } );

            }

            catch
            {
                TempData["ArchiveDeleteResult"] = "false";
                TempData["ArchiveDeleteMessage"] = "El archivo no pudo eliminado";
                throw;
                

            }

            
        }

        // GET: Archives
        public async Task<IActionResult> Index()
        {
            return View(await _context.Archives.Where(a => a.Entity == "InvoicesTralix" || a.Entity == "ExchangeRate" || a.Entity == "Invoice").OrderByDescending(a => a.UploadDate).ToListAsync());
        }

        // GET: Archives/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var archive = await _context.Archives
                .FirstOrDefaultAsync(m => m.ArchiveId == id);
            if (archive == null)
            {
                return NotFound();
            }

            return View(archive);
        }

        // GET: Archives/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Archives/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Archive archive)
        {

           
            if (ModelState.IsValid)
            {
                _context.Add(archive);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(archive);
        }

        // GET: Archives/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var archive = await _context.Archives.FindAsync(id);
            if (archive == null)
            {
                return NotFound();
            }
            return View(archive);
        }

        // POST: Archives/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArchiveId,ArchiveGuid")] Archive archive)
        {
            if (id != archive.ArchiveId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(archive);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArchiveExists(archive.ArchiveId))
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
            return View(archive);
        }

        // GET: Archives/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var archive = await _context.Archives
                .FirstOrDefaultAsync(m => m.ArchiveId == id);
            if (archive == null)
            {
                return NotFound();
            }

            return View(archive);
        }

        // POST: Archives/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int ArchiveId)
        {
            var archive = await _context.Archives.FindAsync(ArchiveId);
            int returnentityId = 0;
            var controller = "";
            var action = "";

            switch (archive.Entity)
            {
                case "Quote":
                    {
                        var id = archive.EntityId; ///Este es el Id de la cotizacion pero tenmos que enviar el id de la Quote
                        var quote = await _context.Quotes
                                                   .FirstOrDefaultAsync(q => q.QuoteId == id);
                        returnentityId = quote.QuoteId;
                        controller = "Quotes";
                        action = "Index";
                        break;
                    }
                case "QuoteDetail" : 
                    {
                        var id = archive.EntityId; ///Este es el Id de QuoteDetail pero tenmos que enviar el id de la Quote
                        var quotedetail = await _context.QuoteDetails
                                                    .Include(qd => qd.Quote)
                                                    .FirstOrDefaultAsync(qd => qd.QuoteDetailId == id);
                        returnentityId = quotedetail.Quote.QuoteId;
                        controller = "Quotes";
                        action = "Details";
                        break;

                    }
                case "WorkOrder":
                    {
                        var id = archive.EntityId; ///Este es el Id de la cotizacion pero tenmos que enviar el id de la Quote
                        var workorder = await _context.WorkOrders
                                                   .FirstOrDefaultAsync(w => w.WorkOrderId == id);
                        returnentityId = workorder.WorkOrderId;
                        controller = "WorkOrders";
                        action = "Index";
                        break;
                    }
                case "WorkOrderDetail":
                    {
                        var id = archive.EntityId; ///Este es el Id de QuoteDetail pero tenmos que enviar el id de la Quote
                        var workorderdetail = await _context.WorkOrderDetails
                                                    .Include(wod => wod.WorkOrder)
                                                    .FirstOrDefaultAsync(wod => wod.WorkOrderDetailId == id);
                        returnentityId = workorderdetail.WorkOrder.WorkOrderId;
                        controller = "WorkOrders";
                        action = "Details";
                        break;

                    }
                case "Order":
                    {
                        var id = archive.EntityId; ///Este es el Id de la cotizacion pero tenmos que enviar el id de la Quote
                        var order = await _context.Orders
                                                   .FirstOrDefaultAsync(o => o.OrderId == id);
                        returnentityId = order.OrderId;
                        controller = "Orders";
                        action = "Index";
                        break;
                    }
                case "OrderDetail":
                    {
                        var id = archive.EntityId; ///Este es el Id de QuoteDetail pero tenmos que enviar el id de la Quote
                        var orderdetail = await _context.OrderDetails
                                                    .Include(od => od.Order)
                                                    .FirstOrDefaultAsync(od => od.OrderDetailId == id);
                        returnentityId = orderdetail.Order.OrderId;
                        controller = "Orders";
                        action = "Details";
                        break;

                    }
            }

            await _blobHelper.DeleteBlobAsync(archive.ArchiveGuid, "archives");
            
            if (archive != null)
            {
                _context.Archives.Remove(archive);
            }

            await _context.SaveChangesAsync();
            TempData["ArchiveDeleteResult"] = "true";
            TempData["ArchiveDeleteMessage"] = "El archivo fué eliminado";
            return RedirectToAction(action, controller, new {id= returnentityId });
        }


        //Este proceso es exclusivo para Finanzas y sirve para leer un excel y poder subir su info a la BD
        public IActionResult UploadOwnInvoices(string entity = "SimaqInvoice", int entityid = 0)
        {
            AddArchiveViewModel model = new AddArchiveViewModel();
            model.Entity = entity;
            model.EntityId = entityid;
         
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]///Este proceso es usado para almacenar archivos y dejarlo guardado ya que casi no se utilizará la funcion eliminar
        //
        public async Task<IActionResult> UploadOwnInvoices(List<IFormFile> ArchiveFileList)
        {
            //El siguiente proceso se ejecuta seguen el siguiente orden:

            string yearmonth_ini = "";///Este sera la fecha inicial mínima para eliminar los registros de InvoicesCompacted
            string yearmonth_fin = "";///Este sera la fecha final máxima para eliminar los registros de InvoicesCompacted

            Guid archiveguid = Guid.Empty;

            foreach (var file in ArchiveFileList)
            {
                //Cargamos el archivo al blob
                //leemos el archivo desde el blob y lo dejamos en memoria

                archiveguid = await _blobHelper.UploadBlobAsync(file, "xmlowninvoices");


                string connectionstring = _configuration["Blob:ConnectionString"];
                BlobClient blobClient = new BlobClient(connectionstring, "xmlowninvoices", archiveguid.ToString());

                Archive archive = new Archive();
                archive.ArchiveGuid = archiveguid;
                archive.Entity = "Invoice";
                archive.EntityId = 1;
                archive.UploadDate = DateTime.Now;
                archive.ArchiveName = file.FileName;
                _context.Add(archive);
                await _context.SaveChangesAsync();

                using (var stream = new MemoryStream()) {
                    var contentType = (blobClient.GetProperties()).Value.ContentType;
                    blobClient.DownloadTo(stream);
                    stream.Position = 0;///esta linea es la que nos ayuda a empezar a leer el lector "reader"
                    ///Vamos a revisar si el header de la factura ya esta cargada
                    ///Si lo esta, tenemos que eliminar el header para volver a subirlo
                    ///Si no esta solo creamos el header y luego subimos las partidas
                    ///Hay que obtener el rango de fechas a eliminar de la tabla InvoicesCompacted y lo tomaremos de la año y mes de la factura mas nueva y mas vieja en el listado de archivos(facturas) que estamos subiendo
                    ///Por ultimo actualizamos el concentrado de facturas pero este proceso lo vamos a hacer al final del foreach de los archivos subidos
                    ///
                    XmlTextReader reader = new XmlTextReader(stream);
                    XmlSerializer serializer = new XmlSerializer(typeof(Comprobante));
                    Comprobante factura = (Comprobante)serializer.Deserialize(reader);
                    string uuid = factura.Complemento.Any[0].Attributes[2].Value;

                    Invoice facturaexistente = await _context.Invoices.FirstOrDefaultAsync(i => i.UUID == uuid);
                    if (facturaexistente != null)
                    {
                        if (facturaexistente.UUID == uuid)
                        {
                            _context.Invoices.Remove(facturaexistente);
                            await _context.SaveChangesAsync();

                            List<InvoiceDetail> lstDetails = _context.InvoiceDetails.Where(d => d.UUID == uuid).ToList();   
                            foreach (InvoiceDetail detail in lstDetails)
                            {
                                _context.InvoiceDetails.Remove(detail);
                                await _context.SaveChangesAsync();
                            }

                        };
                    }

                    //validamos si realmente es una factura con el tipo de comprobante = I

                    if (factura.TipoDeComprobante.ToString() == "I") {

                        Invoice NuevaFactura = new Invoice();


                        NuevaFactura.Serie = factura.Serie;
                        NuevaFactura.Folio = Convert.ToInt32(factura.Folio);
                        NuevaFactura.UUID = uuid;
                        NuevaFactura.EmisionDate = factura.Fecha;
                        NuevaFactura.EmisorTaxId = factura.Emisor.Rfc;
                        NuevaFactura.EmisorName = factura.Emisor.Nombre;
                        NuevaFactura.ReceptorTaxId = factura.Receptor.Rfc;
                        NuevaFactura.Subtotal = factura.SubTotal;
                        NuevaFactura.TrasladedTax = factura.Impuestos.TotalImpuestosTrasladados;
                        NuevaFactura.Total = factura.Total;
                        NuevaFactura.Currency = factura.Moneda.ToString();
                        NuevaFactura.ReceptorName = factura.Receptor.Nombre;
                        NuevaFactura.TaxStatus = "VIGENTE";  //Lo estamos suponiendo porque no viene en el XML
                        NuevaFactura.PayStatus = "NO PAGADO"; //Lo estamos suponiendo porque no viene en el XML
                        NuevaFactura.RecipeType = "Factura"; //Lo estamos suponiendo porque no viene en el XML
                        NuevaFactura.CfdiType = factura.TipoDeComprobante.ToString();
                        NuevaFactura.PayDate = null;
                        NuevaFactura.payComment = null;
                        NuevaFactura.payMethod = factura.MetodoPago.ToString();
                        NuevaFactura.LoadedDate = DateTime.Now;
                        NuevaFactura.LoadType = "XML";



                        string yearmonth = "";
                        string mes = "00";
                        string anio = "";

                        anio = factura.Fecha.Year.ToString();
                        mes += factura.Fecha.Month.ToString();
                        mes = mes.Substring(mes.Length - 2, 2);

                        yearmonth = $"{anio}-{mes}";


                        //Aqui obtenemos el año y mes maximos y minimos
                        if (yearmonth_ini == "")
                        {
                            yearmonth_ini = yearmonth;
                        }
                        else {
                            if (Convert.ToInt32(yearmonth) < Convert.ToInt32(yearmonth_ini) )
                            {
                                yearmonth_ini = yearmonth;
                            } 
                        }

                        if (yearmonth_fin == "")
                        {
                            yearmonth_fin = yearmonth;
                        }
                        else {
                            if (Convert.ToInt32(yearmonth) > Convert.ToInt32(yearmonth_fin))
                            {
                                yearmonth_fin = yearmonth;
                            }
                        }


                        _context.Add(NuevaFactura);
                        await _context.SaveChangesAsync();




                        foreach (ComprobanteConcepto partida in factura.Conceptos)
                        {
                            InvoiceDetail detail = new InvoiceDetail();
                            detail.InvoiceId = NuevaFactura.InvoiceId;
                            detail.UUID = uuid;
                            detail.Cantidad = partida.Cantidad;
                            detail.PU = partida.ValorUnitario;
                            detail.Importe = partida.Importe;
                            detail.Description = partida.Descripcion;
                            detail.UM = partida.Unidad;
                            detail.Folio = NuevaFactura.Folio;
                            _context.Add(detail);
                            await _context.SaveChangesAsync();

                        }

                    }

                    
                  
                }


                //Con yearmonth_ini diferente de "" quiere decir que si entro a guardar una factura nueva y hay que recalcular InvoicedCompacted
                //dentro de las fechas ini y fin


                if (yearmonth_ini != "") {

                    var result = await _context.Database.ExecuteSqlRawAsync("DELETE FROM InvoicesCompacted WHERE CONVERT(nvarchar,Year([Date])) +'-' + RIGHT('0' + CONVERT(nvarchar,Month([Date])),2) >= '" + yearmonth_ini + "' AND CONVERT(nvarchar,Year([Date])) +'-' + RIGHT('0' + CONVERT(nvarchar,Month([Date])),2) <= '" + yearmonth_fin + "')");
                    result = await _context.SaveChangesAsync();

                    string command = "INSERT INTO InvoicesCompacted (Day,Month,Year,Subtotal,Total,[RFC Receptor],[Date])\r\n\t\tSELECT DAY(I.[Fecha de Emisión]) AS Dia,\r\n\t\t\t   MONTH(I.[Fecha de Emisión]) AS Mes,\r\n\t\t\t   YEAR(I.[Fecha de Emisión]) AS Año,\r\n\t\tSUM(CASE I.Moneda\r\n\t\t\tWHEN 'USD' THEN Subtotal*E.Exchangerate\r\n\t\t\tELSE Subtotal\r\n\t\tEND) AS Subtotal,\r\n\t\tSUM(CASE I.Moneda\r\n\t\t\tWHEN 'USD' THEN Total*E.Exchangerate\r\n\t\t\tELSE Total\r\n\t\tEND) As Total,\r\n\t\t[RFC Receptor], \r\n\t\t I.[Fecha de Emisión]\r\n\t\t FROM Invoices I\r\n\t\tLEFT JOIN ExchangeRates E ON (CONVERT(date,I.[Fecha de Emisión]) = E.[Date]  CONVERT(nvarchar,Year([Date])) +'-' + RIGHT('0' + CONVERT(nvarchar,Month([Date])),2) >= '" + yearmonth_ini + "' AND CONVERT(nvarchar,Year([Date])) +'-' + RIGHT('0' + CONVERT(nvarchar,Month([Date])),2) <= '" + yearmonth_fin + "'))\r\n\t\t\tWHERE [Tipo de Comprobante] = 'FACTURA'\r\n\t\tAND [Estado Fiscal] IN ('ACTIVO','VIGENTE','VIGENTE:NO CANCELABLE', 'VIGENTE:SIN ACEPTACIÓN', 'VIGENTE:CON ACEPTACIÓN')\r\n\t\tGroup By  [RFC Receptor],I.[Fecha de Emisión]\r\n";
                    result = await _context.Database.ExecuteSqlRawAsync(command);
                    result = await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Archives");

                }
            }

         

           





            return View();
        }

        private bool ArchiveExists(int id)
        {
            return _context.Archives.Any(e => e.ArchiveId == id);
        }


        private int? ToNullableInt(string s)
        {
            int i;
            if (Int32.TryParse(s, out i)) return i;
            return null;
        }

        private DateTime? ToNullableDate(string s)
        {
            if(s != null && s != "")
            {
                DateTime dt;
                var cultureInfo = new CultureInfo("es-MX");
                dt = DateTime.Parse(s, cultureInfo);
                return dt;
            }
           
            return null;
        }

        private Decimal? ToNullableDecimal(string s)
        {
            Decimal i;
            if (Decimal.TryParse(s, out i)) return i;
            return null;
        }


    }


}
