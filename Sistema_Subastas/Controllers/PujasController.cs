
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RealizarPuja(int articuloId, decimal monto)
        {
            var usuarioId = 1; // Obtén el ID del usuario autenticado desde el contexto de usuario
            var articulo = await _context.articulos.FindAsync(articuloId);

            // Verificar que el usuario está autenticado
            if (usuarioId == 0) // Si el usuario no está autenticado
            {
                return RedirectToAction("Login", "Account");
            }

            // Validaciones
            if (articulo == null)
            {
                TempData["Error"] = "El artículo no existe.";
                return RedirectToAction("Index", "Articulos");
            }

            if (articulo.usuario_id == usuarioId)
            {
                TempData["Error"] = "No puedes pujar por un artículo que tú mismo has publicado.";
                return RedirectToAction("Details", "Articulos", new { id = articuloId });
            }

            if (articulo.fecha_fin < DateTime.Now)
            {
                TempData["Error"] = "La subasta ya ha finalizado.";
                return RedirectToAction("Details", "Articulos", new { id = articuloId });
            }

            var pujaActual = await _context.pujas
                .Where(p => p.articulo_id == articuloId)
                .OrderByDescending(p => p.monto)
                .FirstOrDefaultAsync();

            if (pujaActual != null && monto <= pujaActual.monto)
            {
                TempData["Error"] = "La puja debe ser mayor que la puja actual.";
                return RedirectToAction("Create", new { articuloId });
            }

            if (monto < articulo.precio_salida)
            {
                TempData["Error"] = "La puja debe ser mayor o igual al precio de salida.";
                return RedirectToAction("Create", new { articuloId });
            }

            // Mostrar mensaje de confirmación antes de guardar
            if (Request.Form["confirmar"] == "true")
            {
                var nuevaPuja = new pujas
                {
                    articulo_id = articuloId,
                    usuario_id = usuarioId,
                    monto = monto,
                    fecha_puja = DateTime.Now
                };

                _context.Add(nuevaPuja);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Puja realizada con éxito.";
                return RedirectToAction("Details", "Articulos", new { id = articuloId });
            }

            return View(new { articuloId, monto });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarPuja(int id)
        {
            var puja = await _context.pujas.FindAsync(id);

            if (puja == null)
            {
                TempData["Error"] = "Puja no encontrada.";
                return RedirectToAction("Index");
            }

            var articulo = await _context.articulos.FindAsync(puja.articulo_id);

            if (articulo == null || articulo.fecha_fin < DateTime.Now)
            {
                TempData["Error"] = "La subasta ha finalizado y no se puede cancelar la puja.";
                return RedirectToAction("Details", "Articulos", new { id = articulo.Id });
            }

            if (puja.usuario_id != 1) // Verificar si el usuario que está intentando cancelar la puja es el propietario de la misma
            {
                TempData["Error"] = "No puedes cancelar una puja que no realizaste.";
                return RedirectToAction("Details", "Articulos", new { id = articulo.Id });
            }

            // Mostrar mensaje de confirmación antes de cancelar
            if (Request.Form["confirmar"] == "true")
            {
                _context.pujas.Remove(puja);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Puja cancelada con éxito.";
                return RedirectToAction("Details", "Articulos", new { id = articulo.Id });
            }

            return View(puja);
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
