using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAGM.Models;
using System.Diagnostics;

namespace SAGM.Controllers
{

    [Authorize(Roles = "Administrador,Vendedor")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {

            if (TempData["TwoFactorEnabledResult"] != null)
            {

                ViewBag.Result = TempData["TwoFactorEnabledResult"].ToString();
                ViewBag.Message = TempData["TwoFactorEnabledMessage"].ToString();
                TempData.Remove("TwoFactorEnabledResult");
                TempData.Remove("TwoFactorEnabledMessage");
            }
            ;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("error/404")]
        public IActionResult Error404()
        {
            return View();
        }
    }
}