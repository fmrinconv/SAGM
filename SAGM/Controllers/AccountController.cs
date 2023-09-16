using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAGM.Data;
using SAGM.Helpers;
using SAGM.Models;
using SAGM.Enums;
using SAGM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using NuGet.Protocol.Plugins;
using SAGM.Common;
using NuGet.Packaging.Signing;

namespace SAGM.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly SAGMContext _context;
        private readonly IComboHelper _comboHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IMailHelper _mailHelper;

        public AccountController(IUserHelper userHelper, 
                                SAGMContext context, 
                                IComboHelper comboHelper, 
                                IBlobHelper blobHelper,
                                IMailHelper mailHelper)
        {
            _userHelper = userHelper;
            _context = context;
            _comboHelper = comboHelper;
            _blobHelper = blobHelper;
            _mailHelper = mailHelper;
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.OldPassword == model.NewPassword)
                {
                    ModelState.AddModelError(string.Empty, "Debes ingresar una contraseña diferente.");
                    return View(model);
                }

                User? user = await _userHelper.GetUserAsync(User.Identity.Name);
                if (user != null)
                {
                    IdentityResult result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ChangeUser");
                    }
                    else
                    {
                        if (result.Errors.FirstOrDefault().Description == "Incorrect password.")
                        {
                            ModelState.AddModelError(string.Empty, "Contraseña incorrecta");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault().Description);
                        }
                        
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Usuario no encontrado");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> ChangeUser()
        {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                EditUserViewModel model = new()
                {
                    Id = user.Id,
                    Document = user.Document,
                    Address = user.Address,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    ImageId = user.ImageId,
                    CityId = user.City.CityId,
                    StateId = user.City.State.StateId,
                    CountryId = user.City.State.Country.CountryId,
                    Cities = await _comboHelper.GetComboCitiesAsync(user.City.State.StateId),
                    States = await _comboHelper.GetComboStatesAsync(user.City.State.Country.CountryId),
                    Countries = await _comboHelper.GetComboCountriesAsync()
                };
                return View(model);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> ChangeUser(EditUserViewModel model)
        {
            ViewBag.Result = "false";
            if (ModelState.IsValid)
            {
                Guid imageId = model.ImageId;

                if (model.ImageFile != null)
                {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                }

                model.ImageId = imageId;

                User user = await _userHelper.GetUserAsync(User.Identity.Name);
                user.Document = model.Document;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.City = await _context.Cities.FindAsync(model.CityId);
                await _userHelper.UpdateUserAsync(user);
                ViewBag.Result = "true";
                ViewBag.Message = "El cambio de datos ha sido exitoso";

                model.Countries = await _comboHelper.GetComboCountriesAsync();
                model.States = await _comboHelper.GetComboStatesAsync(model.CountryId);
                model.Cities = await _comboHelper.GetComboCitiesAsync(model.StateId);
                return View(model);

            }
            else
            {
                model.Countries = await _comboHelper.GetComboCountriesAsync();
                model.States = await _comboHelper.GetComboStatesAsync(model.CountryId);
                model.Cities = await _comboHelper.GetComboCitiesAsync(model.StateId);

                return View(model);
            }
        }


        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel());

        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                SignInResult result = await _userHelper.LoginAsync(model);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Ha superado el número máximo de intentos, la cuenta se desbloqueará en 5 min para que vuelva a intentarlo");
                }
                else if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, "Usuario no ha confirmado cuenta, debe seguir las instrucciones del correo enviado " +
                        "para la confirmación y así habilitar su cuenta.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Cuenta o contraseña inválidas.");
                }
            }
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult NotAuthorized()
        {

            return View();
        }

        public async Task<IActionResult> Register() {
            AddUserViewModel model = new() {
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

        public async Task<IActionResult> Register(AddUserViewModel model) {
            if (ModelState.IsValid) {

                Guid imageid = Guid.Empty;

                if (model.ImageFile != null) {
                    imageid = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                }

                model.ImageId = imageid;

                User user = await _userHelper.AddUserAsync(model);

                if (user == null) {
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
                },protocol: HttpContext.Request.Scheme);

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
                    ViewBag.Result = "true";
                    model = new()
                    {
                        Address = "",
                        Username = "",
                        Document = "",
                        FirstName = "",
                        LastName = "",
                        PhoneNumber = "",
                        Id = Guid.Empty.ToString(),
                        Countries = await _comboHelper.GetComboCountriesAsync(),
                        States = await _comboHelper.GetComboStatesAsync(0),
                        Cities = await _comboHelper.GetComboCitiesAsync(0),
                        UserType = UserType.User
                    };
                    ViewBag.Username = "";
                    return View(model);
                }
                
                ModelState.AddModelError(string.Empty, response.Message);
                
            }

            model.Countries = await _comboHelper.GetComboCountriesAsync();
            model.States = await _comboHelper.GetComboStatesAsync(model.CountryId);
            model.Cities = await _comboHelper.GetComboCitiesAsync(model.StateId);
            return View(model);
        }


        public JsonResult GetStates(int countryId) {
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


        public async Task<IActionResult> ConfirmEmail(string userid, string token)
        {
            if (string.IsNullOrEmpty(userid) || string.IsNullOrEmpty(token))
            {
                return NotFound();
            }
            User user = await _userHelper.GetUserAsync(new Guid(userid));
            if (user == null)
            {
                return NotFound();
            }

            IdentityResult result = await _userHelper.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return NotFound();
            }

            return View();
        }

        public IActionResult RecoveryPassword(string email)
        {
            ViewBag.Result = "false";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecoveryPassword(RecoveryPasswordViewModel model)
        {
            ViewBag.Result = "false";
            if (ModelState.IsValid)
            {
                User user = await _userHelper.GetUserAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "El email no corresponde a ningún usuario registrado.");
                    return View(model);
                }
                else {
                    string myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
                    string tokenLink = Url.Action("ResetPassword", 
                        "Account", 
                        new  {token = myToken }, protocol: HttpContext.Request.Scheme);

                    Response response = _mailHelper.SendMail(
                        $"{user.FullName}",
                        model.Email,
                        "SAGEM - Recuperación de contraseña",
                        $"<h1>SAGEM - Recuperación de contraseña</h1>" +
                            $"Para recuperar la contraseña haga click en el siguiente elace:, " +
                            $"<hr/><br/><p><a href = \"{tokenLink}\">Cambiar contraseña</a></p>");

                    if (response.IsSuccess)
                    {
                        ViewBag.Result = "true";
                        return View();
                    }
                }
            }
            return View();
        }

        public IActionResult ResetPassword(string token)
        {
            return View();  
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Result = "false";
                User user = await _userHelper.GetUserAsync(model.Email);
                if (user != null)
                {
                    IdentityResult result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        ViewBag.Message = "Contraseña cambiarda con éxito.";
                        ViewBag.Result = "true";
                        return View();
                    }
                    else
                    {
                        ViewBag.Message = "Error al intentar cambiar contraseña.";
                        return View();
                    }
                }
                else {
                    ViewBag.Message = "Usuario no encontrado.";
                    return View();
                }
                    
                }
        else {
                 ViewBag.Message = "Error al intentar cambiar contraseña.";
                    return View(model);
             }
        }


        public IActionResult NotfyTest()
        { return View(); }

        [HttpPost]
        public async Task<IActionResult> NotfyTest(int? id)
        {
            ViewBag.Result = "true";
            return View();
        }
    }
}
