using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;


namespace SAGM.Controllers
{
    [Authorize(Roles = "Administrador,Usuario,Comprador,Vendedor")]
    public class ReportsController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public ReportsController(IWebHostEnvironment webHostEnvironment) { 
            _environment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Print() 
        {
            string mimetype = "";
            int extension  = 1;
            var path = $"{this._environment.WebRootPath}\\Reports\\SampleReport.rdlc";
            var parameters = new[] { new ReportParameter("param1", "RDLC sample") };
            string renderFormat = "PDF";
           
            LocalReport localReport = new LocalReport();    
            localReport.ReportPath = path;
            localReport.SetParameters(parameters);

            var pdf = localReport.Render(renderFormat);
            return File(pdf, mimetype);

        }
    }
}
