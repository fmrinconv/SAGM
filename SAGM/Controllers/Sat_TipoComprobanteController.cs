using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SAGM.Data;
using SAGM.Data.Entities;

namespace SAGM.Controllers
{
    public class Sat_TipoComprobanteController : Controller
    {
        private readonly SAGMContext _context;

        public Sat_TipoComprobanteController(SAGMContext context)
        {
            _context = context;
        }

        // GET: Sat_TipoComprobante
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sat_TipoComprobante.ToListAsync());
        }

        // GET: Sat_TipoComprobante/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sat_TipoComprobante = await _context.Sat_TipoComprobante
                .FirstOrDefaultAsync(m => m.TipoComprobanteId == id);
            if (sat_TipoComprobante == null)
            {
                return NotFound();
            }

            return View(sat_TipoComprobante);
        }

        // GET: Sat_TipoComprobante/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sat_TipoComprobante/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TipoComprobanteId,TipoComprobante,TipoComprobanteDesc")] Sat_TipoComprobante sat_TipoComprobante)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sat_TipoComprobante);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sat_TipoComprobante);
        }

        // GET: Sat_TipoComprobante/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sat_TipoComprobante = await _context.Sat_TipoComprobante.FindAsync(id);
            if (sat_TipoComprobante == null)
            {
                return NotFound();
            }
            return View(sat_TipoComprobante);
        }

        // POST: Sat_TipoComprobante/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TipoComprobanteId,TipoComprobante,TipoComprobanteDesc")] Sat_TipoComprobante sat_TipoComprobante)
        {
            if (id != sat_TipoComprobante.TipoComprobanteId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sat_TipoComprobante);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Sat_TipoComprobanteExists(sat_TipoComprobante.TipoComprobanteId))
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
            return View(sat_TipoComprobante);
        }

        // GET: Sat_TipoComprobante/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sat_TipoComprobante = await _context.Sat_TipoComprobante
                .FirstOrDefaultAsync(m => m.TipoComprobanteId == id);
            if (sat_TipoComprobante == null)
            {
                return NotFound();
            }

            return View(sat_TipoComprobante);
        }

        // POST: Sat_TipoComprobante/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sat_TipoComprobante = await _context.Sat_TipoComprobante.FindAsync(id);
            if (sat_TipoComprobante != null)
            {
                _context.Sat_TipoComprobante.Remove(sat_TipoComprobante);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Sat_TipoComprobanteExists(int id)
        {
            return _context.Sat_TipoComprobante.Any(e => e.TipoComprobanteId == id);
        }
    }
}
