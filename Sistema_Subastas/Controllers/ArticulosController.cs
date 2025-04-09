using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_Subastas.Models;

namespace Sistema_Subastas.Controllers
{
    public class ArticulosController : Controller
    {
        private readonly subastaDbContext _context;

        public ArticulosController(subastaDbContext context)
        {
            _context = context;
        }

        // GET: Articulos
        public async Task<IActionResult> Index()
        {
            return View(await _context.articulos.ToListAsync());
        }

        // GET: Articulos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var articulo = await _context.articulos
                .FirstOrDefaultAsync(m => m.Id == id);

            if (articulo == null) return NotFound();

            return View(articulo);
        }

        // GET: Articulos/Create
        public IActionResult Create()
        {
            ViewBag.Categorias = _context.categorias.ToList();
            return View();
        }

        // POST: Articulos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,titulo,descripcion,estado,precio_salida,precio_venta,fecha_inicio,fecha_fin,usuario_id,estado_subasta,visualizacion_puja,fecha_registro")] articulos articulos, int categoria_id, int userId)
        {
            articulos.usuario_id = userId;
            articulos.fecha_registro = DateTime.Now;
            articulos.estado_subasta = "Publicado";
            articulos.Id = 0;

            if (ModelState.IsValid)
            {
                _context.Add(articulos);
                await _context.SaveChangesAsync();

                int articuloId = articulos.Id;

                if (categoria_id > 0)
                {
                    var articuloCategoria = new articulo_categoria
                    {
                        articulo_id = articuloId,
                        categoria_id = categoria_id,
                    };

                    _context.articulo_categoria.Add(articuloCategoria);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Create", "Imagenes_articulos", new { articulo_id = articuloId });
            }

            ViewBag.Categorias = _context.categorias.ToList(); // ← importante si fallan validaciones
            return View(articulos);
        }

        // GET: Articulos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var articulo = await _context.articulos.FindAsync(id);
            if (articulo == null) return NotFound();

            ViewBag.Categorias = _context.categorias.ToList();
            return View(articulo);
        }

        // POST: Articulos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,titulo,descripcion,estado,precio_salida,precio_venta,fecha_inicio,fecha_fin,usuario_id,estado_subasta,visualizacion_puja,fecha_registro")] articulos articulos)
        {
            if (id != articulos.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(articulos);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!articulosExists(articulos.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categorias = _context.categorias.ToList(); // ← si hay error en formulario
            return View(articulos);
        }

        // GET: Articulos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var articulo = await _context.articulos
                .FirstOrDefaultAsync(m => m.Id == id);

            if (articulo == null) return NotFound();

            return View(articulo);
        }

        // POST: Articulos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var articulo = await _context.articulos.FindAsync(id);
            if (articulo != null)
            {
                _context.articulos.Remove(articulo);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool articulosExists(int id)
        {
            return _context.articulos.Any(e => e.Id == id);
        }
    }
}
