using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAGM.Data;
using SAGM.Data.Entities;
using SAGM.Helpers;
using SAGM.Models;
using static SAGM.Helpers.ModalHelper;

namespace SAGM.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProcessesController : Controller
    {
        private readonly SAGMContext _context;

        public ProcessesController(SAGMContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {

            ViewBag.Result = "";
            ViewBag.Message = "";

            if (TempData["ProcessDeleteResult"] != null)
            {
                ViewBag.Result = TempData["ProcessDeleteResult"].ToString();
                ViewBag.Message = TempData["ProcessDeleteMessage"].ToString();
                TempData.Remove("ProcessDeleteResult");
                TempData.Remove("ProcessDeleteMessage");
            }

            if (TempData["AddOrEditProcessResult"] != null)
            {
                ViewBag.Result = TempData["AddOrEditProcessResult"].ToString();
                ViewBag.Message = TempData["AddOrEditProcessMessage"].ToString();
                TempData.Remove("AddOrEditProcessResult");
                TempData.Remove("AddOrEditProcessMessage");
            }
            return View(await _context.Processes
                .Include(p => p.Machines)
                .ToListAsync());
        }

        [NoDirectAccess]
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                Process process = new Process();
                process.Active = true;
                return View(process);
            }
            else
            {
                Process process = await _context.Processes.FindAsync(id);
                if (process == null)
                {
                    return NotFound();
                }

                return View(process);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, Process process)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (id == 0) //Insert
                    {
                        _context.Add(process);
                        await _context.SaveChangesAsync();
                        TempData["AddOrEditProcessResult"] = "true";
                        TempData["AddOrEditProcessMessage"] = "El proceso fué agregado";
                    }
                    else //Update
                    {
                        _context.Update(process);
                        await _context.SaveChangesAsync();
                        TempData["AddOrEditProcessResult"] = "true";
                        TempData["AddOrEditProcessMessage"] = "El proceso fué actualizado";
                        // _flashMessage.Info("Registro actualizado.");
                    }
                    return Json(new
                    {
                        isValid = true,
                        html = ModalHelper.RenderRazorViewToString(
                            this,
                            "_ViewAllProcesses",
                            _context.Processes
                                .Include(p => p.Machines)
                                .ToList())
                    });
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        // _flashMessage.Danger("Ya existe un país con el mismo nombre.");
                        ModelState.AddModelError(string.Empty, "Ya existe un proceso con el mismo nombre.");
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEdit", process) });
                    }
                    else
                    {
                        TempData["AddOrEditProcessResult"] = "false";
                        TempData["AddOrEditProcessMessage"] = dbUpdateException.InnerException.Message;
                    }
                }
                catch (Exception exception)
                {
                    TempData["AddOrEditProcessResult"] = "false";
                    TempData["AddOrEditProcessMessage"] = exception.Message.ToString();
                    ModelState.AddModelError(string.Empty, exception.Message.ToString());
                }
                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllProcesses", _context.Processes.ToList()) });
            }

            return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "AddOrEdit", process) });
        }

        [NoDirectAccess]
        public async Task<IActionResult> Delete(int id)
        {
            Process process = await _context.Processes
                .Include(p => p.Machines)
                .FirstOrDefaultAsync(p => p.ProcessId == id);

            try
            {
                _context.Processes.Remove(process);
                await _context.SaveChangesAsync();
                TempData["ProcessDeleteResult"] = "true";
                TempData["ProcessDeleteMessage"] = "El proceso fué eliminado";

            }

            catch
            {
                TempData["ProcessDeleteResult"] = "false";
                TempData["ProcessDeleteMessage"] = "El proceso no pudo ser eliminado";
                throw;

            }
            return RedirectToAction(nameof(Index));
        }

        [NoDirectAccess]
        public async Task<IActionResult> AddMachine(int id)
        {
            Process process = await _context.Processes.FindAsync(id);
            if (process == null)
            {
                return NotFound();
            }

            MachineViewModel model = new()
            {
                ProcessId = process.ProcessId,
                Active = true
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMachine(MachineViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Machine machine = new()
                    {
                        Process = await _context.Processes.FindAsync(model.ProcessId),
                        MachineName = model.MachineName,
                        Active = model.Active
                    };
                    _context.Add(machine);
                    await _context.SaveChangesAsync();
                    TempData["MachineAddResult"] = "true";
                    TempData["MachineAddMessage"] = "La Máquina fué agregada";

                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllMachines", _context.Processes.ToList()) });
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe una máquina con el mismo nombre en este proceso.");
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddState", model) });
                    }
                    else
                    {

                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message.ToString());
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddMachine", model) });
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
        public async Task<IActionResult> EditMachine(int id)
        {
            Machine machine = await _context.Machines
                .Include(m => m.Process)
                .FirstOrDefaultAsync(m => m.MachineId == id);
            if (machine == null)
            {
                return NotFound();
            }

            MachineViewModel model = new()
            {
                ProcessId = machine.Process.ProcessId,
                MachineId = machine.MachineId,
                MachineName = machine.MachineName,
                Active = machine.Active
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMachine(int id, MachineViewModel model)
        {
            if (id != model.MachineId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    try
                    {
                        Machine machine = new()
                        {
                            MachineId = model.MachineId,
                            MachineName = model.MachineName,
                            Active = model.Active
                        };

                        _context.Update(machine);
                        await _context.SaveChangesAsync();
                        TempData["MachineEditResult"] = "true";
                        TempData["MachineEditMessage"] = "La máquina fué actualizada";
                        return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllMachines", new { Id = model.ProcessId }) });
                    }
                    catch (DbUpdateException dbUpdateException)
                    {
                        if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, "Ya existe una máquina con el mismo nombre para el proceso");
                            return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditMachine", model) });
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message.ToString());
                            return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditMachine", model) });
                        }
                    }
                    catch (Exception exception)
                    {
                        ModelState.AddModelError(string.Empty, exception.Message);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MachineExists(model.MachineId))
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
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditState", model) });

            }
            return View(model);
        }

        [NoDirectAccess]
        public async Task<IActionResult> DeleteMachine(int id)
        {
            Machine machine = await _context.Machines
                .Include(m => m.Process)
                .FirstOrDefaultAsync(m => m.MachineId == id);

            try
            {
                _context.Machines.Remove(machine);
                await _context.SaveChangesAsync();
                TempData["MachineDeleteResult"] = "true";
                TempData["MachineDeleteMessage"] = "La máquina fué eliminada";
            }
            catch
            {
                TempData["MachineDeleteResult"] = "false";
                TempData["MachineDeleteMessage"] = "La máquina no pudo ser eliminada";
                return RedirectToAction(nameof(Details), new { id = machine.Process.ProcessId });
            }

            return RedirectToAction(nameof(Details), new { id = machine.Process.ProcessId });
        }

        public async Task<IActionResult> Details(int? id)
        {
            ViewBag.Result = "";
            ViewBag.Message = "";

            if (TempData["MachineDeleteResult"] != null)
            {

                ViewBag.Result = TempData["MachineDeleteResult"].ToString();
                ViewBag.Message = TempData["MachineDeleteMessage"].ToString();
                TempData.Remove("MachineDeleteResult");
                TempData.Remove("MachineDeleteMessage");
            }

            if (TempData["MachineEditResult"] != null)
            {

                ViewBag.Result = TempData["MachineEditResult"].ToString();
                ViewBag.Message = TempData["MachineEditMessage"].ToString();
                TempData.Remove("MachineEditResult");
                TempData.Remove("MachineEditMessage");
            }
            if (TempData["MachineAddResult"] != null)
            {

                ViewBag.Result = TempData["MachineAddResult"].ToString();
                ViewBag.Message = TempData["MachineAddMessage"].ToString();
                TempData.Remove("MachineAddResult");
                TempData.Remove("MachineAddMessage");
            }

            if (id == null)
            {
                return NotFound();
            }

            Process process = await _context.Processes
                .Include(p => p.Machines)
                .FirstOrDefaultAsync(p => p.ProcessId == id);
            if (process == null)
            {
                return NotFound();
            }

            return View(process);
        }


        private bool MachineExists(int id)
        {
            return (_context.Machines?.Any(s => s.MachineId == id)).GetValueOrDefault();
        }



    }
}
