using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAGM.Data;
using SAGM.Data.Entities;
using SAGM.Helpers;
using SAGM.Models;
using static SAGM.Helpers.ModalHelper;

namespace SAGM.Controllers
{


    public class ArchivesController : Controller
    {
        private readonly SAGMContext _context;
        private readonly IBlobHelper _blobHelper;
        private readonly IUserHelper _userHelper;
        private readonly IComboHelper _comboHelper;


        public ArchivesController(SAGMContext context, IBlobHelper blobHelper, IUserHelper userHelper, IComboHelper comboHelper)
        {
            _context = context;
            _blobHelper = blobHelper;
            _userHelper = userHelper;
            _comboHelper = comboHelper;
        }

        public IActionResult AddArchive(string entity = "", int entityid=0)
        {
            AddArchiveViewModel model = new AddArchiveViewModel();
            model.Entity = entity;
            model.EntityId = entityid;
            return View(model);
        }




        [HttpPost]
        public async Task<IActionResult> AddArchive(AddArchiveViewModel model)
        {
            Guid archiveguid = Guid.Empty;

            if (model.ArchiveFile != null)
            {
                archiveguid = await _blobHelper.UploadBlobAsync(model.ArchiveFile, "archives");
            }

            model.ArchiveGuid = archiveguid;

            Archive archive = new Archive();
            archive.ArchiveGuid = archiveguid;
            archive.Entity = model.Entity;
            archive.EntityId = model.EntityId;
            archive.ArchiveName = model.ArchiveFile.FileName;
            _context.Add(archive);
            await _context.SaveChangesAsync();



            switch (archive.Entity)
            {
                case "QuoteDetail":
                    TempData["AddArchiveResult"] = "true";
                    TempData["AddArchiveMessage"] = "La carga de archivos se ha completado con éxito";
                    QuoteDetail qd = await _context.QuoteDetails
                                        .Include(qd => qd.Quote)
                                        .FirstOrDefaultAsync(qd => qd.QuoteDetailId == archive.EntityId);

                    return RedirectToAction("Details", "Quotes", new { id = qd.Quote.QuoteId, quoteDetilId = qd.QuoteDetailId });

                case "Quote":
                    TempData["AddArchiveResult"] = "true";
                    TempData["AddArchiveMessage"] = "La carga de archivos se ha completado con éxito";
                    Quote q = await _context.Quotes
                                        .FirstOrDefaultAsync(q => q.QuoteId == archive.EntityId);

                    return RedirectToAction("Index", "Quotes", new { id = q.QuoteId });

                default:
                    break;
            }

            

            return View(archive);
        }


        [NoDirectAccess]
        [HttpPost]
        public async Task<IActionResult> DeleteArchive(int id, string controller, int entityId)
        {


            Archive archive = await _context.Archives
                .FirstOrDefaultAsync(m => m.ArchiveId == id);

            try
            {
                _context.Archives.Remove(archive);
                await _context.SaveChangesAsync();
                await _blobHelper.DeleteBlobAsync(archive.ArchiveGuid, "archives");
                return Json(new { isValid = true, success = true } );

            }

            catch
            {
                TempData["ArchiveDeleteResult"] = "false";
                TempData["ArchiveDeleteMessage"] = "El archivo no pudo eliminado";
                throw;
                

            }

            
        }

        // GET: Archives
        public async Task<IActionResult> Index()
        {
            return View(await _context.Archives.ToListAsync());
        }

        // GET: Archives/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var archive = await _context.Archives
                .FirstOrDefaultAsync(m => m.ArchiveId == id);
            if (archive == null)
            {
                return NotFound();
            }

            return View(archive);
        }

        // GET: Archives/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Archives/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Archive archive)
        {

           
            if (ModelState.IsValid)
            {
                _context.Add(archive);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(archive);
        }

        // GET: Archives/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var archive = await _context.Archives.FindAsync(id);
            if (archive == null)
            {
                return NotFound();
            }
            return View(archive);
        }

        // POST: Archives/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArchiveId,ArchiveGuid")] Archive archive)
        {
            if (id != archive.ArchiveId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(archive);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArchiveExists(archive.ArchiveId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(archive);
        }

        // GET: Archives/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var archive = await _context.Archives
                .FirstOrDefaultAsync(m => m.ArchiveId == id);
            if (archive == null)
            {
                return NotFound();
            }

            return View(archive);
        }

        // POST: Archives/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int ArchiveId)
        {
            var archive = await _context.Archives.FindAsync(ArchiveId);
            int returnentityId = 0;
            var controller = "";
            var action = "";

            switch (archive.Entity)
            {
                case "QuoteDetail" : 
                    {
                        var id = archive.EntityId; ///Este es el Id de QuoteDetail pero tenmos que enviar el id de la Quote
                        var quotedetail = await _context.QuoteDetails
                                                    .Include(qd => qd.Quote)
                                                    .FirstOrDefaultAsync(qd => qd.QuoteDetailId == id);
                        returnentityId = quotedetail.Quote.QuoteId;
                        controller = "Quotes";
                        action = "Details";
                        break;

                    }
                case "Quote":
                    {
                        var id = archive.EntityId; ///Este es el Id de la cotizacion pero tenmos que enviar el id de la Quote
                        var quote = await _context.Quotes
                                                   .FirstOrDefaultAsync(q => q.QuoteId == id);
                        returnentityId = quote.QuoteId;
                        controller = "Quotes";
                        action = "Index";
                        break;
                    }
            }

            await _blobHelper.DeleteBlobAsync(archive.ArchiveGuid, "archives");
            
            if (archive != null)
            {
                _context.Archives.Remove(archive);
            }

            await _context.SaveChangesAsync();
            TempData["ArchiveDeleteResult"] = "true";
            TempData["ArchiveDeleteMessage"] = "El archivo fué eliminado";
            return RedirectToAction(action, controller, new {id= returnentityId });
        }

        private bool ArchiveExists(int id)
        {
            return _context.Archives.Any(e => e.ArchiveId == id);
        }
    }


}
