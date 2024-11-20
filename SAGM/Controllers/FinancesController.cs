using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAGM.Data;
using SAGM.Data.Entities;
using SAGM.Helpers;
using SAGM.Models;
using System.Globalization;

namespace SAGM.Controllers
{
    public class FinancesController : Controller
    {
        private readonly SAGMContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IComboHelper _comboHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IConfiguration _configuration;
        private readonly IReportHelper _reportHelper;

        public FinancesController(SAGMContext context, IUserHelper userHelper, IComboHelper comboHelper, IBlobHelper blobHelper, IConfiguration configuration, IReportHelper reportHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _comboHelper = comboHelper;
            _blobHelper = blobHelper;
            _configuration = configuration;
            _reportHelper = reportHelper;
        }
        public IActionResult Billing()
        {
            var cultureInfo = new CultureInfo("es-MX");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            BillingGraph billingGraph = new BillingGraph();
            billingGraph.DateIni = DateOnly.FromDateTime(DateTime.Now).AddMonths(-1);
            billingGraph.DateFin = DateOnly.FromDateTime(DateTime.Now);
            return View(billingGraph);
        }


        public async Task<JsonResult> GetBilling(DateOnly fini, DateOnly ffin)
        {
            List<GraphicPay> serie = new List<GraphicPay>();
            GraphicPay paydata = new GraphicPay();


            string fechaIni = fini.ToString().Substring(0, 4) + fini.ToString().Substring(5, 2) + fini.ToString().Substring(8, 2);
            string fechaFin = ffin.ToString().Substring(0, 4) + ffin.ToString().Substring(5, 2) + ffin.ToString().Substring(8, 2);

            //DateOnly dateini = new DateOnly(Convert.ToInt32(fini.Substring(0, 4)), Convert.ToInt32(fini.Substring(5, 2)), Convert.ToInt32(fini.Substring(8, 2)));
            //DateOnly datefin = new DateOnly(Convert.ToInt32(ffin.Substring(0, 4)), Convert.ToInt32(ffin.Substring(5, 2)), Convert.ToInt32(ffin.Substring(8, 2)));

            List<InvoicesCompacted> LstInvoices = _context.InvoicesCompacted.Where(d => d.Date >= fini && d.Date <= ffin).ToList();
            //List<InvoicesCompacted> LstInvoices = _context.InvoicesCompacted
            //                                      .FromSql($"SELECT * FROM InvoicesCompacted WHERE CONVERT(nvarchar,[Year]) + RIGHT('0' + CONVERT(nvarchar,[Month]),2) +  RIGHT('0'+ CONVERT(nvarchar,[day]),2) >= {fechaIni} AND  CONVERT(nvarchar,[Year]) + RIGHT('0' + CONVERT(nvarchar,[Month]),2) +  RIGHT('0'+ CONVERT(nvarchar,[day]),2) <= {fechaFin}" ).ToList();



            //Hacemos un SUM(Subtotal) agrupado por GroupBy(ReceptorTaxId)

            var LstInvoicesSummary = LstInvoices.GroupBy(l => l.ReceptorTaxId).Select(r => new
            {
                RFC = r.Key,
                Subtotal = r.Sum(s => s.Subtotal)
            }).ToList();

            decimal sumtotal = 0;
            foreach (var invoice in LstInvoicesSummary) {

                Customer cust = await _context.Customers.FirstOrDefaultAsync(c => c.TaxId == invoice.RFC);
                String Customer = "";

                if (cust == null)
                {
                    Customer = invoice.RFC;
                }
                else
                {
                    Customer = cust.CustomerNickName.ToString();
                }

               paydata = new GraphicPay();
                paydata.x = Customer;
               paydata.value = invoice.Subtotal;
               serie.Add(paydata);
                sumtotal += decimal.Parse(paydata.value.ToString()); 
            }


        

            return Json(new { serie = serie.OrderByDescending(x => x.value), total = sumtotal });
        }

        
    
    }
}
