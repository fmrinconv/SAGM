using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAGM.Data;
using SAGM.Data.Entities;
using SAGM.Models;
using SAGM.Helpers; 
using static SAGM.Helpers.ModalHelper;

namespace SAGM.Controllers
{
    [Authorize(Roles = "Administrador")]
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
            ViewBag.Result = "";
            ViewBag.Message = "";

            if (TempData["CategoryDeleteResult"] != null)
            {
                ViewBag.Result = TempData["CategoryDeleteResult"].ToString();
                ViewBag.Message = TempData["CategoryDeleteMessage"].ToString();
                TempData.Remove("CategoryDeleteResult");
                TempData.Remove("CategoryDeleteMessage");
            }

            if (TempData["AddOrEditCategoryResult"] != null)
            {
                ViewBag.Result = TempData["AddOrEditCategoryResult"].ToString();
                ViewBag.Message = TempData["AddOrEditCategoryMessage"].ToString();
                TempData.Remove("AddOrEditCategoryResult");
                TempData.Remove("AddOrEditCategoryMessage");
            }

            return View(await _context.Categories
                .Include(c => c.MaterialTypes.OrderBy(s => s.MaterialTypeName))
                .ThenInclude(mt => mt.Materials)
                .ToListAsync());

        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewBag.Result = "";
            ViewBag.Message = "";

            if (TempData["MaterialTypeDeleteResult"] != null)
            {

                ViewBag.Result = TempData["MaterialTypeDeleteResult"].ToString();
                ViewBag.Message = TempData["MaterialTypeDeleteMessage"].ToString();
                TempData.Remove("MaterialTypeDeleteResult");
                TempData.Remove("MaterialTypeDeleteMessage");
            }

            if (TempData["MaterialTypeEditResult"] != null)
            {

                ViewBag.Result = TempData["MaterialTypeEditResult"].ToString();
                ViewBag.Message = TempData["MaterialTypeEditMessage"].ToString();
                TempData.Remove("MaterialTypeEditResult");
                TempData.Remove("MaterialTypeEditMessage");
            }
            if (TempData["MaterialTypeAddResult"] != null)
            {

                ViewBag.Result = TempData["MaterialTypeAddResult"].ToString();
                ViewBag.Message = TempData["MaterialTypeAddMessage"].ToString();
                TempData.Remove("MaterialTypeAddResult");
                TempData.Remove("MaterialTypeAddMessage");
            }



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
            ViewBag.Result = "";
            ViewBag.Message = "";

            if (TempData["MaterialAddResult"] != null)
            {
                ViewBag.Result = TempData["MaterialAddResult"].ToString();
                ViewBag.Message = TempData["MaterialAddMessage"].ToString();
                TempData.Remove("MaterialAddResult");
                TempData.Remove("MaterialAddMessage");
            }

            if (TempData["MaterialEditResult"] != null)
            {

                ViewBag.Result = TempData["MaterialEditResult"].ToString();
                ViewBag.Message = TempData["MaterialEditMessage"].ToString();
                TempData.Remove("MaterialEditResult");
                TempData.Remove("MaterialEditMessage");
            }
            if (TempData["MaterialDeleteResult"] != null)
            {

                ViewBag.Result = TempData["MaterialDeleteResult"].ToString();
                ViewBag.Message = TempData["MaterialDeleteMessage"].ToString();
                TempData.Remove("MaterialDeleteResult");
                TempData.Remove("MaterialDeleteMessage");
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



        [NoDirectAccess]
        public async Task<IActionResult> AddOrEditCategory(int id = 0)
        {
            if (id == 0)
            {
                Category category = new Category();
                category.Active = true;
                return View(category);
            }
            else
            {
                Category category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEditCategory(int id, Category category)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    if (id == 0)
                    {
                        _context.Add(category);
                        await _context.SaveChangesAsync();
                        TempData["AddOrEditCategoryResult"] = "true";
                        TempData["AddOrEditCategoryMessage"] = "La categoría fué agregada";
                    }
                    else
                    {
                        _context.Update(category);
                        await _context.SaveChangesAsync();
                        TempData["AddOrEditCategoryResult"] = "true";
                        TempData["AddOrEditCategoryMessage"] = "La categoría fué actualizada";
                    }
                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllCategories", _context.Categories.ToList()) });

                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe una categoría con el mismo nombre en este estado");
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEditCategory", category) });
                    }
                    else
                    {
                        TempData["AddOrEditCategoryResult"] = "false";
                        TempData["AddOrEditCategoryMessage"] = dbUpdateException.InnerException.Message;
                    }

                }

                catch (Exception exception)
                {
                    TempData["AddOrEditCategoryResult"] = "false";
                    TempData["AddOrEditCategoryResult"] = exception.Message;

                }
                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllCategories", _context.Categories.ToList()) });

            }
            else
            {
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEditCategory", category) });
            }

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
                    TempData["MaterialTypeAddResult"] = "true";
                    TempData["MaterialTypeAddMessage"] = "El tipo de material fue agregado";
                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllMaterialTypes", _context.Categories.ToList()) });
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {

                        ModelState.AddModelError(string.Empty, "Ya existe un tipo de material con el mismo nombre");
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddMaterialType", materialtypemodel) });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message.ToString());
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddMaterialType", materialtypemodel) });
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
                    TempData["MaterialAddResult"] = "true";
                    TempData["MaterialAddMessage"] = "El material fué agregado";
                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllMaterials", _context.MaterialTypes.ToList()) });
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe un material con el mismo nombre");
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddMaterial", materialmodel) });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                        return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddMaterial", materialmodel) });
                    }
                }
                catch (Exception exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);

                }
            }
            return View(materialmodel);
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
            Category category = await _context.Categories.FindAsync(id);
            MaterialType materialtype = new();
            materialtype = new()
            {
                MaterialTypeId = model.MaterialTypeId,
                MaterialTypeName = model.MaterialTypeName,
                Category = category,
                Active = model.Active
            };



            if (ModelState.IsValid)
            {
                try
                {
                    try
                    {

                        _context.Update(materialtype);
                        await _context.SaveChangesAsync();
                        TempData["MaterialTypeEditResult"] = "true";
                        TempData["MaterialTypeEditMessage"] = "El tipo de material fué actualizado";
                        return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllMaterialTypes", new { Id = model.CategoryId }) });
                    }
                    catch (DbUpdateException dbUpdateException)
                    {
                        if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, "Ya existe un tipo de material con el mismo nombre");
                            return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditMaterialType", model) });
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message.ToString());
                            return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditMaterialType", model) });
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
            else
            {
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditMaterialType", model) });

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

            MaterialType materialtype = await _context.MaterialTypes.FindAsync(id);
            Material material = new()
            {
                MaterialType = materialtype,
                MaterialName = model.MaterialName,
                Active = model.Active
            };

            if (ModelState.IsValid)
            {
                try
                {
                    try
                    {

                        _context.Update(material);
                        await _context.SaveChangesAsync();
                        TempData["MaterialEditResult"] = "true";
                        TempData["MaterialEditMessage"] = "El material fué actualizado";
                        return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllMaterials", new { Id = model.MaterialTypeId }) });
                    }
                    catch (DbUpdateException dbUpdateException)
                    {
                        if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, "Ya existe un material con el mismo nombre");
                            return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditMaterial", model) });
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message.ToString());
                            return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditMaterial", model) });
                        }
                    }
                    catch (Exception exception)
                    {
                        ModelState.AddModelError(string.Empty, exception.Message);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialExists(model.MaterialTypeId))
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
                return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditMaterial", model) });

            }
            return View(model);



        }
        // GET: Categories/Delete/5
        [NoDirectAccess]
        public async Task<IActionResult> Delete(int? id)
        {


            Category Category = await _context.Categories
                .Include(c => c.MaterialTypes)
                .ThenInclude(mt => mt.Materials)
                .FirstOrDefaultAsync(m => m.CategoryId == id);

            try
            {
                _context.Categories.Remove(Category);
                await _context.SaveChangesAsync();
                TempData["CategoryDeleteResult"] = "true";
                TempData["CategoryDeleteMessage"] = "La categoría fué eliminada";

            }

            catch
            {
                TempData["CategoryDeleteResult"] = "false";
                TempData["CategoryDeleteMessage"] = "La categoría no pudo ser eliminada";
                throw;

            }

            return RedirectToAction(nameof(Index));
        }

        [NoDirectAccess]
        public async Task<IActionResult> DeleteMaterialType(int? id)
        {

            var materialtype = await _context.MaterialTypes
                .Include(s => s.Materials)
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.MaterialTypeId == id);
            try
            {
                _context.MaterialTypes.Remove(materialtype);
                await _context.SaveChangesAsync();
                TempData["MaterialTypeDeleteResult"] = "true";
                TempData["MaterialTypeDeleteMessage"] = "El tipo de material fué eliminado";
            }

            catch
            {
                TempData["MaterialTypeDeleteResult"] = "false";
                TempData["MaterialTypeDeleteMessage"] = "El tipo de material no pudo ser eliminado";
                return RedirectToAction(nameof(Details), new { id = materialtype.Category.CategoryId });

            }

            return RedirectToAction(nameof(Details), new { id = materialtype.Category.CategoryId });
       

        }

        public async Task<IActionResult> DeleteMaterial(int? id)
        {

            var material = await _context.Materials
                .Include(m => m.MaterialType)
                .FirstOrDefaultAsync(m => m.MaterialId == id);
            try
            {
                _context.Materials.Remove(material);
                await _context.SaveChangesAsync();
                TempData["MaterialDeleteResult"] = "true";
                TempData["MaterialDeleteMessage"] = "El material fué eliminado";
            }

            catch
            {
                TempData["MaterialDeleteResult"] = "false";
                TempData["MaterialDeleteMessage"] = "El material no pudo ser eliminado";
                return RedirectToAction(nameof(DetailsMaterialType), new { id = material.MaterialType.MaterialTypeId });

            }

            return RedirectToAction(nameof(DetailsMaterialType), new { id = material.MaterialType.MaterialTypeId });

        }


        private bool CategoryExists(int id)
        {
            return (_context.Categories?.Any(e => e.CategoryId == id)).GetValueOrDefault();
        }

        private bool MaterialTypeExists(int id)
        {
            return (_context.MaterialTypes?.Any(e => e.MaterialTypeId == id)).GetValueOrDefault();
        }

        private bool MaterialExists(int id)
        {
            return (_context.Materials?.Any(e => e.MaterialId == id)).GetValueOrDefault();
        }
    }
}
