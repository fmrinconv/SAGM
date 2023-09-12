using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAGM.Data;
using SAGM.Helpers;
using SAGM.Models;
using SAGM.Enums;
using SAGM.Data.Entities;
using SAGM.Common;

namespace SAGM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly SAGMContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IComboHelper _comboHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IMailHelper _mailHelper;

        public UsersController(SAGMContext context, 
                               IUserHelper userHelper, 
                               IComboHelper comboHelper,
                               IBlobHelper blobHelper,
                               IMailHelper mailHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _comboHelper = comboHelper;
            _blobHelper = blobHelper;
            _mailHelper = mailHelper;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Users
                .Include(u => u.City)
                .ThenInclude(c => c.State)
                .ThenInclude(s => s.Country)
                .ToListAsync());
        }

   

        public async Task<IActionResult> Create()
        {
            AddUserViewModel model = new()
            {
                Id = Guid.Empty.ToString(),
                Countries = await _comboHelper.GetComboCountriesAsync(),
                States = await _comboHelper.GetComboStatesAsync(0),
                Cities = await _comboHelper.GetComboCitiesAsync(0),
                UserType = UserType.User
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(AddUserViewModel model)
        {
            if (ModelState.IsValid)
            {

                Guid imageid = Guid.Empty;

                if (model.ImageFile != null)
                {
                    imageid = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                }

                model.ImageId = imageid;

                User user = await _userHelper.AddUserAsync(model);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Este correo ya esta siendo usado");
                    model.Countries = await _comboHelper.GetComboCountriesAsync();
                    model.States = await _comboHelper.GetComboStatesAsync(model.CountryId);
                    model.Cities = await _comboHelper.GetComboCitiesAsync(model.StateId);
                    return View(model);
                }

                string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                string tokenLink = Url.Action("ConfirmEmail", "Account", new
                {
                    userid = user.Id,
                    token = myToken
                }, protocol: HttpContext.Request.Scheme);

                Response response = _mailHelper.SendMail(
                    $"{model.FirstName} {model.LastName}",
                    model.Username,
                    "SAGEM - Confirmación de Email",
                    $"<h1>SAGEM - Confirmación de Email</h1>" +
                        $"Para habilitar el usuario por favor hacer click en el siguiente link:, " +
                        $"<hr/><br/><p><a href = \"{tokenLink}\">Confirmar Email</a></p>");

                if (response.IsSuccess)
                {
                    ViewBag.Message = "Las instrucciones para habilitar el usuario han sido enviadas al correo.";
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, response.Message);

            }

            return RedirectToAction("Index");
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

    }
}
