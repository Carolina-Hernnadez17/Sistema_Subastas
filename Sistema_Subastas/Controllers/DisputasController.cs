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
    public class DisputasController : Controller
    {
        private readonly subastaDbContext _context;

        public DisputasController(subastaDbContext context)
        {
            _context = context;
        }

        // GET: Disputas
        public async Task<IActionResult> Index()
        {
            return View(await _context.disputas.ToListAsync());
        }

        // GET: Disputas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var disputas = await _context.disputas
                .FirstOrDefaultAsync(m => m.id == id);
            if (disputas == null)
            {
                return NotFound();
            }

            return View(disputas);
        }

        // GET: Disputas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Disputas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,articulo_id,comprador_id,vendedor_id,motivo,descripcion,estado,fecha")] disputas disputas)
        {
            if (ModelState.IsValid)
            {
                _context.Add(disputas);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(disputas);
        }

        // GET: Disputas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var disputas = await _context.disputas.FindAsync(id);
            if (disputas == null)
            {
                return NotFound();
            }
            return View(disputas);
        }

        // POST: Disputas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,articulo_id,comprador_id,vendedor_id,motivo,descripcion,estado,fecha")] disputas disputas)
        {
            if (id != disputas.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(disputas);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!disputasExists(disputas.id))
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
            return View(disputas);
        }

        // GET: Disputas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var disputas = await _context.disputas
                .FirstOrDefaultAsync(m => m.id == id);
            if (disputas == null)
            {
                return NotFound();
            }

            return View(disputas);
        }

        // POST: Disputas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var disputas = await _context.disputas.FindAsync(id);
            if (disputas != null)
            {
                _context.disputas.Remove(disputas);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool disputasExists(int id)
        {
            return _context.disputas.Any(e => e.id == id);
        }
    }
}
