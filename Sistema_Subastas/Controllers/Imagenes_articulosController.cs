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
    public class Imagenes_articulosController : Controller
    {
        private readonly subastaDbContext _context;

        public Imagenes_articulosController(subastaDbContext context)
        {
            _context = context;
        }

        // GET: Imagenes_articulos
        public async Task<IActionResult> Index()
        {
            return View(await _context.imagenes_articulos.ToListAsync());
        }

        // GET: Imagenes_articulos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var imagenes_articulos = await _context.imagenes_articulos
                .FirstOrDefaultAsync(m => m.id == id);
            if (imagenes_articulos == null)
            {
                return NotFound();
            }

            return View(imagenes_articulos);
        }

        // GET: Imagenes_articulos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Imagenes_articulos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,articulo_id,url_imagen")] imagenes_articulos imagenes_articulos)
        {
            if (ModelState.IsValid)
            {
                _context.Add(imagenes_articulos);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(imagenes_articulos);
        }

        // GET: Imagenes_articulos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var imagenes_articulos = await _context.imagenes_articulos.FindAsync(id);
            if (imagenes_articulos == null)
            {
                return NotFound();
            }
            return View(imagenes_articulos);
        }

        // POST: Imagenes_articulos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,articulo_id,url_imagen")] imagenes_articulos imagenes_articulos)
        {
            if (id != imagenes_articulos.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(imagenes_articulos);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!imagenes_articulosExists(imagenes_articulos.id))
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
            return View(imagenes_articulos);
        }

        // GET: Imagenes_articulos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var imagenes_articulos = await _context.imagenes_articulos
                .FirstOrDefaultAsync(m => m.id == id);
            if (imagenes_articulos == null)
            {
                return NotFound();
            }

            return View(imagenes_articulos);
        }

        // POST: Imagenes_articulos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var imagenes_articulos = await _context.imagenes_articulos.FindAsync(id);
            if (imagenes_articulos != null)
            {
                _context.imagenes_articulos.Remove(imagenes_articulos);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool imagenes_articulosExists(int id)
        {
            return _context.imagenes_articulos.Any(e => e.id == id);
        }
    }
}
