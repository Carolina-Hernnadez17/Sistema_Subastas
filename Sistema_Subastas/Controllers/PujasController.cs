using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema_Subastas.Models;

namespace Sistema_Subastas.Controllers
{
    public class PujasController : Controller
    {
        private readonly subastaDbContext _context;

        public PujasController(subastaDbContext context)
        {
            _context = context;
        }

        // GET: Pujas
        public async Task<IActionResult> Index()
        {
            return View(await _context.pujas.ToListAsync());
        }

        // GET: Pujas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pujas = await _context.pujas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pujas == null)
            {
                return NotFound();
            }

            return View(pujas);
        }

        // GET: Pujas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pujas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,articulo_id,usuario_id,monto,fecha_puja")] pujas pujas)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pujas);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pujas);
        }

        // GET: Pujas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pujas = await _context.pujas.FindAsync(id);
            if (pujas == null)
            {
                return NotFound();
            }
            return View(pujas);
        }

        // POST: Pujas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,articulo_id,usuario_id,monto,fecha_puja")] pujas pujas)
        {
            if (id != pujas.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pujas);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!pujasExists(pujas.Id))
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
            return View(pujas);
        }

        // GET: Pujas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pujas = await _context.pujas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pujas == null)
            {
                return NotFound();
            }

            return View(pujas);
        }

        // POST: Pujas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pujas = await _context.pujas.FindAsync(id);
            if (pujas != null)
            {
                _context.pujas.Remove(pujas);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool pujasExists(int id)
        {
            return _context.pujas.Any(e => e.Id == id);
        }
    }
}
