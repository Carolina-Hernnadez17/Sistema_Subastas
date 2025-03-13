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
    public class Mensajes_disputasController : Controller
    {
        private readonly subastaDbContext _context;

        public Mensajes_disputasController(subastaDbContext context)
        {
            _context = context;
        }

        // GET: Mensajes_disputas
        public async Task<IActionResult> Index()
        {
            return View(await _context.mensajes_disputas.ToListAsync());
        }

        // GET: Mensajes_disputas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mensajes_disputas = await _context.mensajes_disputas
                .FirstOrDefaultAsync(m => m.id == id);
            if (mensajes_disputas == null)
            {
                return NotFound();
            }

            return View(mensajes_disputas);
        }

        // GET: Mensajes_disputas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Mensajes_disputas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,desputa_id,usuario_id,mensaje,fecha")] mensajes_disputas mensajes_disputas)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mensajes_disputas);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mensajes_disputas);
        }

        // GET: Mensajes_disputas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mensajes_disputas = await _context.mensajes_disputas.FindAsync(id);
            if (mensajes_disputas == null)
            {
                return NotFound();
            }
            return View(mensajes_disputas);
        }

        // POST: Mensajes_disputas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,desputa_id,usuario_id,mensaje,fecha")] mensajes_disputas mensajes_disputas)
        {
            if (id != mensajes_disputas.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mensajes_disputas);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!mensajes_disputasExists(mensajes_disputas.id))
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
            return View(mensajes_disputas);
        }

        // GET: Mensajes_disputas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mensajes_disputas = await _context.mensajes_disputas
                .FirstOrDefaultAsync(m => m.id == id);
            if (mensajes_disputas == null)
            {
                return NotFound();
            }

            return View(mensajes_disputas);
        }

        // POST: Mensajes_disputas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mensajes_disputas = await _context.mensajes_disputas.FindAsync(id);
            if (mensajes_disputas != null)
            {
                _context.mensajes_disputas.Remove(mensajes_disputas);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool mensajes_disputasExists(int id)
        {
            return _context.mensajes_disputas.Any(e => e.id == id);
        }
    }
}
