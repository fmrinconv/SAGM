using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAGM.Data;
using SAGM.Helpers;
using SAGM.Models;
using SAGM.Enums;
using SAGM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SAGM.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly SAGMContext _context;
        private readonly IComboHelper _comboHelper;
        private readonly IBlobHelper _blobHelper;

        public AccountController(IUserHelper userHelper, SAGMContext context, IComboHelper comboHelper, IBlobHelper blobHelper)
        {
            _userHelper = userHelper;
            _context = context;
            _comboHelper = comboHelper;
            _blobHelper = blobHelper;
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
                User? user = await _userHelper.GetUserAsync(User.Identity.Name);
                if (user != null)
                {
                    IdentityResult result = await _userHelper.ChangeUserPasswordAsync(user, model.OldPassword, model.NewPassword);
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

            if (ModelState.IsValid)
            {

                Guid imageId = model.ImageId;

                if (model.ImageFile != null)
                {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                }

                User user = await _userHelper.GetUserAsync(User.Identity.Name);
                user.Document = model.Document;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.City = await _context.Cities.FindAsync(model.CityId);
                await _userHelper.UpdateUserAsync(user);
                return RedirectToAction("Index", "Home");

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
                Microsoft.AspNetCore.Identity.SignInResult result = await _userHelper.LoginAsync(model);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Cuenta o contraseña inválidos.");

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

                LoginViewModel loginViewModel = new LoginViewModel
                {
                    Password = model.Password,
                    RememberMe = false,
                    Username = model.Username
                };

                var result2 = await _userHelper.LoginAsync(loginViewModel);

                if (result2.Succeeded) {
                    return RedirectToAction("Index", "Home");
                }
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


    }
}
