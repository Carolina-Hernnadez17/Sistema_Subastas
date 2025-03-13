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
    public class Articulo_categoriaController : Controller
    {
        private readonly subastaDbContext _context;

        public Articulo_categoriaController(subastaDbContext context)
        {
            _context = context;
        }

        // GET: Articulo_categoria
        public async Task<IActionResult> Index()
        {
            return View(await _context.articulo_categoria.ToListAsync());
        }

        // GET: Articulo_categoria/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articulo_categoria = await _context.articulo_categoria
                .FirstOrDefaultAsync(m => m.articulo_id == id);
            if (articulo_categoria == null)
            {
                return NotFound();
            }

            return View(articulo_categoria);
        }

        // GET: Articulo_categoria/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Articulo_categoria/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("articulo_id,categoria_id")] articulo_categoria articulo_categoria)
        {
            if (ModelState.IsValid)
            {
                _context.Add(articulo_categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(articulo_categoria);
        }

        // GET: Articulo_categoria/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articulo_categoria = await _context.articulo_categoria.FindAsync(id);
            if (articulo_categoria == null)
            {
                return NotFound();
            }
            return View(articulo_categoria);
        }

        // POST: Articulo_categoria/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("articulo_id,categoria_id")] articulo_categoria articulo_categoria)
        {
            if (id != articulo_categoria.articulo_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(articulo_categoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!articulo_categoriaExists(articulo_categoria.articulo_id))
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
            return View(articulo_categoria);
        }

        // GET: Articulo_categoria/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articulo_categoria = await _context.articulo_categoria
                .FirstOrDefaultAsync(m => m.articulo_id == id);
            if (articulo_categoria == null)
            {
                return NotFound();
            }

            return View(articulo_categoria);
        }

        // POST: Articulo_categoria/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var articulo_categoria = await _context.articulo_categoria.FindAsync(id);
            if (articulo_categoria != null)
            {
                _context.articulo_categoria.Remove(articulo_categoria);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool articulo_categoriaExists(int id)
        {
            return _context.articulo_categoria.Any(e => e.articulo_id == id);
        }
    }
}
