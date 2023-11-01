using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAGM.Data;
using SAGM.Helpers;
using SAGM.Models;
using SAGM.Enums;
using SAGM.Data.Entities;
using SAGM.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using static SAGM.Helpers.ModalHelper;

namespace SAGM.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsersController : Controller
    {
        private readonly SAGMContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IComboHelper _comboHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IConfiguration _configuration;

        public UsersController(SAGMContext context, 
                               IUserHelper userHelper, 
                               IComboHelper comboHelper,
                               IBlobHelper blobHelper,
                               IMailHelper mailHelper,
                               IConfiguration configuration)
        {
            _context = context;
            _userHelper = userHelper;
            _comboHelper = comboHelper;
            _blobHelper = blobHelper;
            _mailHelper = mailHelper;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Result = "";
            ViewBag.Message = "";

            if (TempData["UserDeleteResult"] != null)
            {
                ViewBag.Result = TempData["UserDeleteResult"].ToString();
                ViewBag.Message = TempData["UserDeleteMessage"].ToString();
                TempData.Remove("UserDeleteResult");
                TempData.Remove("UserDeleteMessage");
            }

            if (TempData["UserAddOrEditResult"] != null)
            {
                ViewBag.Result = TempData["UserAddOrEditResult"].ToString();
                ViewBag.Message = TempData["UserAddOrEditMessage"].ToString();
                TempData.Remove("UserAddOrEditResult");
                TempData.Remove("UserAddOrEditMessage");
            }
            return View(await _context.Users
                .Include(u => u.City)
                .ThenInclude(c => c.State)
                .ThenInclude(s => s.Country)
                .ToListAsync());
        }

   

        public async Task<IActionResult> AddOrEdit(string? id )
        {
            AddUserViewModel model ;

           
            if (string.IsNullOrWhiteSpace(id))
            {
                model = new()
                {
                    Id = Guid.Empty.ToString(),
                    Countries = await _comboHelper.GetComboCountriesAsync(),
                    States = await _comboHelper.GetComboStatesAsync(0),
                    Cities = await _comboHelper.GetComboCitiesAsync(0),
                    UserType = UserType.Usuario,
                    Password = _configuration["DefaultPwd:pwd"],
                    PasswordConfirm = _configuration["DefaultPwd:pwd"]
                };
            }
            else {
                User user = await _userHelper.GetUserAsync(User.Identity.Name);


                model = new()
                {
                    Id = user.Id,
                    Email = user.UserName,
                    Username = user.UserName,
                    Document = user.Document,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Address = user.Address,
                    PhoneNumber = user.PhoneNumber,   
                    CityId = user.City.CityId,
                    StateId = user.City.State.StateId,
                    CountryId = user.City.State.Country.CountryId,
                    Cities = await _comboHelper.GetComboCitiesAsync(user.City.State.StateId),
                    States = await _comboHelper.GetComboStatesAsync(user.City.State.Country.CountryId),
                    Countries = await _comboHelper.GetComboCountriesAsync()
                };
       
            }
     
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(AddUserViewModel model)
        {
           
                Guid imageid = Guid.Empty;

                if (model.ImageFile != null)
                {
                    imageid = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                }

                model.ImageId = imageid;

                if (string.IsNullOrWhiteSpace(model.Id))
                {
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
                            $"<hr/><br/><p><a href = \"{tokenLink}\">Confirmar Email</a></p> <BR> Su contraseña termporal es Simaq2011#, es necesario que cambie su contraseña de inmediato");

                    if (response.IsSuccess)
                    {
                        TempData["UserAddOrEditResult"] = "true";
                        TempData["UserAddOrEditMessage"] = "El usuario fué agregado";
                        return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllUsers", new { Id = model.Id }) });
                    }

                    ModelState.AddModelError(string.Empty, "El usuario no pudo ser actualizado" );
                    return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEditUser", model) });
                }
                else {

                    User user = await _userHelper.GetUserAsync(model.Email);
                    user.Address = model.Address;
                    user.Document = model.Document;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.ImageId = model.ImageId;
                    user.City = await _context.Cities.FindAsync(model.CityId);
                    await _userHelper.UpdateUserAsync(user);

                    TempData["UserAddOrEditResult"] = "true";
                    TempData["UserAddOrEditMessage"] = "El usuario fué actualizado";
                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllUsers", new { Id = model.Id }) });

                }

           

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


        public async Task<IActionResult> Details(string? id)
        {
            ViewBag.Result = "";
            ViewBag.Message = "";

            if (TempData["RoleAddResult"] != null)
            {

                ViewBag.Result = TempData["RoleAddResult"].ToString();
                ViewBag.Message = TempData["RoleAddMessage"].ToString();
                TempData.Remove("RoleAddResult");
                TempData.Remove("RoleAddMessage");
            }


            if (TempData["RoleDeleteResult"] != null)
            {

                ViewBag.Result = TempData["RoleDeleteResult"].ToString();
                ViewBag.Message = TempData["RoleDeleteMessage"].ToString();
                TempData.Remove("RoleDeleteResult");
                TempData.Remove("RoleDeleteMessage");
            }


            User user = await _userHelper.GetUserAsync(id);

            var roles = await _userHelper.GetUserRolesAsync(user);

            List<RolesViewModel> list = new List<RolesViewModel>();

            RolesViewModel rolesViewModel;

            foreach (var role in roles) {
                rolesViewModel = new RolesViewModel() { Name = role };
                list.Add(rolesViewModel);
            }



            AddUserViewModel model = new AddUserViewModel()
            {
                Username = user.UserName,
                Address = user.Address,
                Document = user.Document,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Id = user.Id,
                ImageId = user.ImageId,
                CityId = user.City.CityId,
                RolesVM = list

            };


            return View(model);
        }


        public async Task<IActionResult> AddRole(string? id)
        {
          
     
      

            User user = await _userHelper.GetUserAsync(id);

            var roles = await _userHelper.GetRolesForUserAsync(user);

            List<RolesViewModel> list = new List<RolesViewModel>();

            RolesViewModel rolesViewModel;

            foreach (var role in roles)
            {
                rolesViewModel = new RolesViewModel() { Name = role };
                list.Add(rolesViewModel);
            }



            AddUserViewModel model = new AddUserViewModel()
            {
                Username = user.UserName,
                Address = user.Address,
                Document = user.Document,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Id = user.Id,
                ImageId = user.ImageId,
                CityId = user.City.CityId,
                RolesVM = list

            };


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> AddRole(string username, string UserType)
        {

            User user = await _userHelper.GetUserAsync(username);
            try
            {
                await _userHelper.AddUserToRoleAsync(user, UserType);

                TempData["RoleAddResult"] = "true";
                TempData["RoleAddMessage"] = $"El Rol: {UserType} ha sido agregado a {username}" ;

                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "Details", new { id = username }) });
            }
            catch (Exception e)
            {
                var exc = e;

                throw;
            }
         

          


        }


        [NoDirectAccess]
        public async Task<IActionResult> DeleteRole(string id)
        {
            //hay que eliminar el rol al usuario


            string[] data = id.Split("&");
            string username = data[0];
            string rolename = data[1];

            User user = await _userHelper.GetUserAsync(data[0]);

           


            try
            {

                await _userHelper.RemoveFromRoleAsync(user, rolename);
                TempData["RoleDeleteResult"] = "true";
                TempData["RoleDeleteMessage"] = $"El rol: {rolename} fué eliminado de {username}";
            }

            catch
            {
                TempData["RoleDeleteResult"] = "false";
                TempData["RoleDeleteMessage"] = $"El rol: {rolename} no pudo fué eliminado de {username}";
                return RedirectToAction(nameof(Details), new { id = username });

            }

            return RedirectToAction(nameof(Details), new { id = username });

        }

    }
}
