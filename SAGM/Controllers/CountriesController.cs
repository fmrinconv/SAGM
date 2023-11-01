using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SAGM.Data;
using SAGM.Data.Entities;
using SAGM.Models;
using SAGM.Helpers;
using static SAGM.Helpers.ModalHelper;

namespace SAGM.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CountriesController : Controller
    {
        private readonly SAGMContext _context;

        public CountriesController(SAGMContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            ViewBag.Result = "";
            ViewBag.Message = "";

            if (TempData["CountryDeleteResult"] != null)
            {
                ViewBag.Result = TempData["CountryDeleteResult"].ToString();
                ViewBag.Message = TempData["CountryDeleteMessage"].ToString();
                TempData.Remove("CountryDeleteResult");
                TempData.Remove("CountryDeleteMessage");
            }

            if (TempData["AddOrEditCountryResult"] != null)
            {
                ViewBag.Result = TempData["AddOrEditCountryResult"].ToString();
                ViewBag.Message = TempData["AddOrEditCountryMessage"].ToString();
                TempData.Remove("AddOrEditCountryResult");
                TempData.Remove("AddOrEditCountryMessage");
            }
            return View(await _context.Countries
                .Include(c => c.States)
                .ThenInclude(s => s.Cities)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            ViewBag.Result = "";
            ViewBag.Message = "";

            if (TempData["StateDeleteResult"] != null)
            {

                ViewBag.Result = TempData["StateDeleteResult"].ToString();
                ViewBag.Message = TempData["StateDeleteMessage"].ToString();
                TempData.Remove("StateDeleteResult");
                TempData.Remove("StateDeleteMessage");
            }

            if (TempData["StateEditResult"] != null)
            {

                ViewBag.Result = TempData["StateEditResult"].ToString();
                ViewBag.Message = TempData["StateEditMessage"].ToString();
                TempData.Remove("StateEditResult");
                TempData.Remove("StateEditMessage");
            }
            if (TempData["StateAddResult"] != null)
            {

                ViewBag.Result = TempData["StateAddResult"].ToString();
                ViewBag.Message = TempData["StateAddMessage"].ToString();
                TempData.Remove("StateAddResult");
                TempData.Remove("StateAddMessage");
            }

            if (id == null)
            {
                return NotFound();
            }

            Country country = await _context.Countries
                .Include(c => c.States)
                .ThenInclude(s => s.Cities)
                .FirstOrDefaultAsync(m => m.CountryId == id);
            if (country == null)
            {
                return NotFound();
            }

            return View(country);
        }

        public async Task<IActionResult> DetailsState(int? id)
        {
            //---------------------

            ViewBag.Result = "";
            ViewBag.Message = "";

            if (TempData["CityDeleteResult"] != null)
            {

                ViewBag.Result = TempData["CityDeleteResult"].ToString();
                ViewBag.Message = TempData["CityDeleteMessage"].ToString();
                TempData.Remove("CityDeleteResult");
                TempData.Remove("CityDeleteMessage");
            }

            if (TempData["CityEditResult"] != null)
            {

                ViewBag.Result = TempData["CityEditResult"].ToString();
                ViewBag.Message = TempData["CityEditMessage"].ToString();
                TempData.Remove("CityEditResult");
                TempData.Remove("CityEditMessage");
            }
            if (TempData["CityAddResult"] != null)
            {

                ViewBag.Result = TempData["CityAddResult"].ToString();
                ViewBag.Message = TempData["CityAddMessage"].ToString();
                TempData.Remove("CityAddResult");
                TempData.Remove("CityAddMessage");
            }

            if (id == null)
            {
                return NotFound();
            }

            State state = await _context.States
                .Include(s => s.Country)
                .Include(s => s.Cities)
                .FirstOrDefaultAsync(m => m.StateId == id);
            if (state == null)
            {
                return NotFound();
            }

            return View(state);
        }

        [NoDirectAccess]
        public async Task<IActionResult> AddState(int id)
        {
            Country country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            StateViewModel model = new()
            {
                CountryId = country.CountryId,
                Active = true
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddState(StateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    State state = new()
                    {
                        Cities = new List<City>(),
                        Country = await _context.Countries.FindAsync(model.CountryId),
                        StateName = model.StateName,
                        Active = model.Active
                    };
                    _context.Add(state);
                    await _context.SaveChangesAsync();
                    TempData["StateAddResult"] = "true";
                    TempData["StateAddMessage"] = "El estado fué agregado";

                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllStates", _context.Countries.ToList()) });
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe un estado con el mismo nombre en este país.");
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddState", model) });
                    }
                    else
                    {

                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message.ToString());
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddState", model) });
                    }
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }

            return View(model);
        }

        [NoDirectAccess]
        public async Task<IActionResult> AddCity(int id)
        {
            State state = await _context.States.FindAsync(id);
            if (state == null)
            {
                return NotFound();
            }

            CityViewModel model = new()
            {
                StateId = state.StateId,
                Active = true
            };

            return View(model);


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCity(CityViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    City city = new()
                    {
                        State = await _context.States.FindAsync(model.StateId),
                        CityName = model.CityName,
                        Active = model.Active
                    };
                    _context.Add(city);
                    await _context.SaveChangesAsync();
                    await _context.SaveChangesAsync();
                    TempData["CityAddResult"] = "true";
                    TempData["CityAddMessage"] = "La ciudad fué agregada";
                    State state = await _context.States
                        .Include(s => s.Cities)
                        .FirstOrDefaultAsync(c => c.StateId == model.StateId);

                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllCities", state) });
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe una ciudad con el mismo nombre en este Estado.");
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddCity", model) });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddCity", model) });
                    }
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }

            return View(model);
        }

        [NoDirectAccess]
        public async Task<IActionResult> EditState(int id)
        {
            State state = await _context.States
                .Include(s => s.Country)
                .FirstOrDefaultAsync(s => s.StateId == id);
            if (state == null)
            {
                return NotFound();
            }

            StateViewModel model = new()
            {
                CountryId = state.Country.CountryId,
                StateId = state.StateId,
                StateName = state.StateName,
                Active = state.Active
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditState(int id, StateViewModel model)
        {
            if (id != model.StateId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    try
                    {
                        State state = new()
                        {
                            StateId = model.StateId,
                            StateName = model.StateName,
                            Active = model.Active
                        };

                        _context.Update(state);
                        await _context.SaveChangesAsync();
                        TempData["StateEditResult"] = "true";
                        TempData["StateEditMessage"] = "El estado fué actualizado";
                        return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllStates", new { Id = model.CountryId }) });
                    }
                    catch (DbUpdateException dbUpdateException)
                    {
                        if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, "Ya existe un estado con el mismo nombre para el país");
                            return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditState", model) });
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message.ToString());
                            return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditState", model) });
                        }
                    }
                    catch (Exception exception)
                    {
                        ModelState.AddModelError(string.Empty, exception.Message);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StateExists(model.StateId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

            }
            else
            {
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditMaterialType", model) });

            }
            return View(model);
        }

        [NoDirectAccess]
        public async Task<IActionResult> EditCity(int id)
        {
            City city = await _context.Cities
                .Include(c => c.State)
                .FirstOrDefaultAsync(c => c.CityId == id);
            if (city == null)
            {
                return NotFound();
            }

            CityViewModel model = new()
            {
                StateId = city.State.StateId,
                CityId = city.CityId,
                CityName = city.CityName,
                Active = city.Active,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCity(int id, CityViewModel model)
        {
            if (id != model.CityId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    City city = new()
                    {
                        CityId = model.CityId,
                        CityName = model.CityName,
                        Active = model.Active,
                    };
                    _context.Update(city);
                    await _context.SaveChangesAsync();
                    State state = await _context.States
                        .Include(s => s.Cities)
                        .FirstOrDefaultAsync(c => c.StateId == model.StateId);
                        TempData["CityEditResult"] = "true";
                        TempData["CityEditMessage"] = "El estado fué actualizado";
                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllCities", state) });
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe una ciuad con el mismo nombre en este Estado.");
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditCity", model) });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message.ToString());
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditCity", model) });
                    }
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }

            return View(model);
        }

        [NoDirectAccess]
        public async Task<IActionResult> Delete(int id)
        {
            Country country = await _context.Countries
                .Include(c => c.States)
                .ThenInclude(s => s.Cities)
                .FirstOrDefaultAsync(c => c.CountryId == id);

            try
            {
                _context.Countries.Remove(country);
                await _context.SaveChangesAsync();
                TempData["CountryDeleteResult"] = "true";
                TempData["CountryDeleteMessage"] = "El país fué eliminado";

            }

            catch
            {
                TempData["CountryDeleteResult"] = "false";
                TempData["CountryDeleteMessage"] = "El país no pudo ser eliminado";
                throw;

            }
            return RedirectToAction(nameof(Index));
        }

        [NoDirectAccess]
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                Country country = new Country();
                country.Active = true;
                return View(country);
            }
            else
            {
                Country country = await _context.Countries.FindAsync(id);
                if (country == null)
                {
                    return NotFound();
                }

                return View(country);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, Country country)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (id == 0) //Insert
                    {
                        _context.Add(country);
                        await _context.SaveChangesAsync();
                        TempData["AddOrEditCountryResult"] = "true";
                        TempData["AddOrEditCountryMessage"] = "El país fué agregado";
                    }
                    else //Update
                    {
                        _context.Update(country);
                        await _context.SaveChangesAsync();
                        TempData["AddOrEditCountryResult"] = "true";
                        TempData["AddOrEditCountryMessage"] = "El país fué actualizado";
                        // _flashMessage.Info("Registro actualizado.");
                    }
                    return Json(new
                    {
                        isValid = true,
                        html = ModalHelper.RenderRazorViewToString(
                            this,
                            "_ViewAllCountries",
                            _context.Countries
                                .Include(c => c.States)
                                .ThenInclude(s => s.Cities)
                                .ToList())
                    });
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        // _flashMessage.Danger("Ya existe un país con el mismo nombre.");
                        ModelState.AddModelError(string.Empty, "Ya existe un país con el mismo nombre.");
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEdit", country) });
                    }
                    else
                    {
                        // _flashMessage.Danger(dbUpdateException.InnerException.Message);
                        TempData["AddOrEditCountryResult"] = "false";
                        TempData["AddOrEditCountryMessage"] = dbUpdateException.InnerException.Message;
                    }
                }
                catch (Exception exception)
                {
                    // _flashMessage.Danger(exception.Message);
                    TempData["AddOrEditCountryResult"] = "false";
                    TempData["AddOrEditCountryMessage"] = exception.Message.ToString();
                    ModelState.AddModelError(string.Empty, exception.Message.ToString());
                }
                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllCountries", _context.Categories.ToList()) });
            }

            return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "AddOrEdit", country) });
        }

        [NoDirectAccess]
        public async Task<IActionResult> DeleteState(int id)
        {
            State state = await _context.States
                .Include(s => s.Country)
                .FirstOrDefaultAsync(s => s.StateId == id);
      

            try
            {
                _context.States.Remove(state);
                await _context.SaveChangesAsync();
                TempData["StateDeleteResult"] = "true";
                TempData["StateDeleteMessage"] = "El estado fué eliminado";
            }
            catch
            {
                TempData["StateeDeleteResult"] = "false";
                TempData["StateeDeleteMessage"] = "El estado no pudo ser eliminado";
                return RedirectToAction(nameof(Details), new { id = state.Country.CountryId });
            }

            return RedirectToAction(nameof(Details), new { id = state.Country.CountryId });
        }

        [NoDirectAccess]
        public async Task<IActionResult> DeleteCity(int id)
        {
            City city = await _context.Cities
                .Include(c => c.State)
                .FirstOrDefaultAsync(c => c.CityId == id);

            try
            {
                TempData["CityDeleteResult"] = "true";
                TempData["CityDeleteMessage"] = "La ciudad fué eliminada";
                _context.Cities.Remove(city);
                await _context.SaveChangesAsync();
               
            }
            catch
            {
                TempData["CityDeleteResult"] = "false";
                TempData["CityDeleteMessage"] = "La ciudad no pudo ser eliminada";
                return RedirectToAction(nameof(DetailsState), new { id = city.State.StateId });
            }

            return RedirectToAction(nameof(DetailsState), new { id = city.State.StateId });
        }


        private bool CountryExsts(int id)
        {
            return (_context.Countries?.Any(c => c.CountryId == id)).GetValueOrDefault();
        }

        private bool StateExists(int id)
        {
            return (_context.States?.Any(s => s.StateId == id)).GetValueOrDefault();
        }

        private bool CityExists(int id)
        {
            return (_context.Cities?.Any(c => c.CityId == id)).GetValueOrDefault();
        }
    }
}
