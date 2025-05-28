using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema_Subastas.Atributo;
using Sistema_Subastas.Models;

namespace Sistema_Subastas.Controllers
{
    [SesionActiva]

    public class Seguimiento_subastasController : Controller
    {
        private readonly subastaDbContext _context;

        public Seguimiento_subastasController(subastaDbContext context)
        {
            _context = context;
        }

        // GET: Seguimiento_subastas
        public async Task<IActionResult> Index()
        {
            return View(await _context.seguimiento_subastas.ToListAsync());
        }

        // GET: Seguimiento_subastas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seguimiento_subastas = await _context.seguimiento_subastas
                .FirstOrDefaultAsync(m => m.id == id);
            if (seguimiento_subastas == null)
            {
                return NotFound();
            }

            return View(seguimiento_subastas);
        }

        // GET: Seguimiento_subastas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Seguimiento_subastas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,usuario_id,articulo_id")] seguimiento_subastas seguimiento_subastas)
        {
            if (ModelState.IsValid)
            {
                _context.Add(seguimiento_subastas);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(seguimiento_subastas);
        }

        // GET: Seguimiento_subastas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seguimiento_subastas = await _context.seguimiento_subastas.FindAsync(id);
            if (seguimiento_subastas == null)
            {
                return NotFound();
            }
            return View(seguimiento_subastas);
        }

        // POST: Seguimiento_subastas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,usuario_id,articulo_id")] seguimiento_subastas seguimiento_subastas)
        {
            if (id != seguimiento_subastas.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(seguimiento_subastas);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!seguimiento_subastasExists(seguimiento_subastas.id))
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
            return View(seguimiento_subastas);
        }

        // GET: Seguimiento_subastas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seguimiento_subastas = await _context.seguimiento_subastas
                .FirstOrDefaultAsync(m => m.id == id);
            if (seguimiento_subastas == null)
            {
                return NotFound();
            }

            return View(seguimiento_subastas);
        }

        // POST: Seguimiento_subastas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var seguimiento_subastas = await _context.seguimiento_subastas.FindAsync(id);
            if (seguimiento_subastas != null)
            {
                _context.seguimiento_subastas.Remove(seguimiento_subastas);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool seguimiento_subastasExists(int id)
        {
            return _context.seguimiento_subastas.Any(e => e.id == id);
        }
    }
}
