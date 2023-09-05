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


namespace SAGM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly SAGMContext _context;

        public CategoriesController(SAGMContext context)
        {
            _context = context;
        }

        // GET: Categories

        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories
                .Include(c => c.MaterialTypes.OrderBy(s => s.MaterialTypeName))
                .ThenInclude(mt => mt.Materials)
                .ToListAsync());

        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var Category = await _context.Categories
                .Include(c => c.MaterialTypes)
                .ThenInclude(s => s.Materials)
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (Category == null)
            {
                return NotFound();
            }

            return View(Category);
        }

        public async Task<IActionResult> DetailsMaterialType(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var materialtype = await _context.MaterialTypes
                .Include(s => s.Category)
                .Include(s => s.Materials.OrderBy(s => s.MaterialName))
                .FirstOrDefaultAsync(m => m.MaterialTypeId == id);
            if (materialtype == null)
            {
                return NotFound();
            }

            return View(materialtype);
        }

        public async Task<IActionResult> DetailsMaterial(int? id)
        {
            if (id == null || _context.Materials == null)
            {
                return NotFound();
            }

            var material = await _context.Materials
                .Include(c => c.MaterialType)
                .FirstOrDefaultAsync(c => c.MaterialId == id);
            if (material == null)
            {
                return NotFound();
            }

            return View(material);
        }

        public IActionResult Create()
        {
            Category Category = new() { MaterialTypes = new List<MaterialType>() };
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category Category)
        {


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(Category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe un país con el mismo nombre");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }
            return View(Category);
        }
        public async Task<IActionResult> AddMaterialType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Category Category = await _context.Categories.FindAsync(id);
            if (Category == null)
            {
                return NotFound();
            }
            MaterialTypeViewModel materialtypemodel = new()
            {
                CategoryId = Category.CategoryId,
            };

            return View(materialtypemodel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterialType(MaterialTypeViewModel materialtypemodel)
        {


            if (ModelState.IsValid)
            {
                try
                {
                    MaterialType materialtype = new()
                    {
                        Materials = new List<Material>(),
                        Category = await _context.Categories.FindAsync(materialtypemodel.CategoryId),
                        MaterialTypeName = materialtypemodel.MaterialTypeName,
                        Active = materialtypemodel.Active

                    };
                    _context.Add(materialtype);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { Id = materialtypemodel.CategoryId });
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe un tipo de material con el mismo nombre en esta categoría");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }
            return View(materialtypemodel);
        }


        public async Task<IActionResult> AddMaterial(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MaterialType materialtype = await _context.MaterialTypes.FindAsync(id);
            if (materialtype == null)
            {
                return NotFound();
            }
            MaterialViewModel materialmodel = new()
            {
                MaterialTypeId = materialtype.MaterialTypeId,
                Active = materialtype.Active
            };

            return View(materialmodel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial(MaterialViewModel materialmodel)
        {


            if (ModelState.IsValid)
            {
                try
                {
                    Material material = new()
                    {
                        MaterialType = await _context.MaterialTypes.FindAsync(materialmodel.MaterialTypeId),
                        MaterialName = materialmodel.MaterialName,
                        Active = materialmodel.Active

                    };
                    _context.Add(material);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(DetailsMaterialType), new { Id = materialmodel.MaterialTypeId });
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe una ciudad con el mismo nombre en este estado");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }
            return View(materialmodel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var Category = await _context.Categories
                .Include(c => c.MaterialTypes)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (Category == null)
            {
                return NotFound();
            }
            return View(Category);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category Category)
        {
            if (id != Category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    try
                    {
                        _context.Update(Category);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateException dbUpdateException)
                    {
                        if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, "Ya existe un país con el mismo nombre");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                        }
                    }
                    catch (Exception exception)
                    {
                        ModelState.AddModelError(string.Empty, exception.Message);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(Category.CategoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

            }
            return View(Category);
        }

        public async Task<IActionResult> EditMaterialType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materialtype = await _context.MaterialTypes
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.MaterialTypeId == id);
            if (materialtype == null)
            {
                return NotFound();
            }

            MaterialTypeViewModel materialtypeViewModel = new()
            {
                CategoryId = materialtype.Category.CategoryId,
                MaterialTypeId = materialtype.MaterialTypeId,
                MaterialTypeName = materialtype.MaterialTypeName,
                Active = materialtype.Active
            };
            return View(materialtypeViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMaterialType(int id, MaterialTypeViewModel model)
        {
            if (id != model.MaterialTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    try
                    {
                        MaterialType materialtype = new()
                        {
                            MaterialTypeId = model.MaterialTypeId,
                            MaterialTypeName = model.MaterialTypeName,
                            Active = model.Active
                        };
                        _context.Update(materialtype);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Details), new { Id = model.CategoryId });
                    }
                    catch (DbUpdateException dbUpdateException)
                    {
                        if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, "Ya existe un estado con el mismo nombre");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                        }
                    }
                    catch (Exception exception)
                    {
                        ModelState.AddModelError(string.Empty, exception.Message);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialTypeExists(model.MaterialTypeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

            }
            return View(model);
        }

        public async Task<IActionResult> EditMaterial(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materials
                .Include(c => c.MaterialType)
                .FirstOrDefaultAsync(c => c.MaterialId == id);
            if (material == null)
            {
                return NotFound();
            }

            MaterialViewModel materialViewModel = new()
            {
                MaterialId = material.MaterialId,
                MaterialTypeId = material.MaterialType.MaterialTypeId,
                MaterialName = material.MaterialName,
                Active = material.Active
            };
            return View(materialViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMaterial(int id, MaterialViewModel model)
        {
            if (id != model.MaterialId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    try
                    {
                        Material material = new()
                        {
                            MaterialId = model.MaterialId,
                            MaterialName = model.MaterialName,
                            Active = model.Active
                        };
                        _context.Update(material);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(DetailsMaterialType), new { Id = model.MaterialTypeId });
                    }
                    catch (DbUpdateException dbUpdateException)
                    {
                        if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, "Ya existe una ciudad con el mismo nombre para el estado");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                        }
                    }
                    catch (Exception exception)
                    {
                        ModelState.AddModelError(string.Empty, exception.Message);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialTypeExists(model.MaterialId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

            }
            return View(model);
        }
        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var Category = await _context.Categories
                .Include(c => c.MaterialTypes)
                .ThenInclude(mt => mt.Materials)
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (Category == null)
            {
                return NotFound();
            }

            return View(Category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'SAGMContext.Categories'  is null.");
            }
            var Category = await _context.Categories.FindAsync(id);
            if (Category != null)
            {
                _context.Categories.Remove(Category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteMaterialType(int? id)
        {
            if (id == null || _context.MaterialTypes == null)
            {
                return NotFound();
            }

            var materialtype = await _context.MaterialTypes
                .Include(s => s.Materials)
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.MaterialTypeId == id);
            if (materialtype == null)
            {
                return NotFound();
            }

            return View(materialtype);
        }

        [HttpPost, ActionName("DeleteMaterialType")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMaterialTypeConfirmed(int id)
        {
            if (_context.MaterialTypes == null)
            {
                return Problem("Entity set 'SAGMContext.Categories'  is null.");
            }
            var materialtype = await _context.MaterialTypes
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.MaterialTypeId == id);
            if (materialtype != null)
            {
                _context.MaterialTypes.Remove(materialtype);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { Id = materialtype.Category.CategoryId });
        }

        private bool CategoryExists(int id)
        {
            return (_context.Categories?.Any(e => e.CategoryId == id)).GetValueOrDefault();
        }

        private bool MaterialTypeExists(int id)
        {
            return (_context.MaterialTypes?.Any(e => e.MaterialTypeId == id)).GetValueOrDefault();
        }
    }
}
