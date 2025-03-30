
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema_Subastas.Models;

namespace Sistema_Subastas.Controllers
{
    public class PujasController : Controller
    {
        private readonly subastaDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PujasController(subastaDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? articuloId)
        {
            if (articuloId == null)
            {
                return View(new List<pujas>()); // Evita consultas innecesarias si no hay artículo
            }

            var pujas = await _context.pujas
                .Where(p => p.articulo_id == articuloId)
                .OrderByDescending(p => p.monto)
                .ToListAsync();

            return View(pujas);
        
        }

        [HttpPost]
        public async Task<IActionResult> RealizarPuja(int articuloId, decimal monto)
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Buscar el usuario en la base de datos por su correo
            var usuario = await _context.usuarios.FirstOrDefaultAsync(u => u.correo == identityUser.Email);
            if (usuario == null)
            {
                TempData["Error"] = "No se encontró el usuario en la base de datos.";
                return RedirectToAction("Details", "Articulos", new { id = articuloId });
            }

            var articulo = await _context.articulos.FindAsync(articuloId);
            if (articulo == null || articulo.fecha_fin < DateTime.Now)
            {
                TempData["Error"] = "No puedes pujar por un artículo inexistente o cuya subasta ha finalizado.";
                return RedirectToAction("Details", "Articulos", new { id = articuloId });
            }

            if (articulo.usuario_id == usuario.id)
            {
                TempData["Error"] = "No puedes pujar por tu propio artículo.";
                return RedirectToAction("Details", "Articulos", new { id = articuloId });
            }

            if (monto <= articulo.precio_salida)
            {
                TempData["Error"] = "La puja debe ser mayor al precio inicial.";
                return RedirectToAction("Details", "Articulos", new { id = articuloId });
            }

            var pujaMasAlta = await _context.pujas
                .Where(p => p.articulo_id == articuloId)
                .OrderByDescending(p => p.monto)
                .FirstOrDefaultAsync();

            if (pujaMasAlta != null && monto <= pujaMasAlta.monto)
            {
                TempData["Error"] = "La puja debe ser mayor a la actual.";
                return RedirectToAction("Details", "Articulos", new { id = articuloId });
            }

            var nuevaPuja = new pujas
            {
                articulo_id = articuloId,
                usuario_id = usuario.id,
                monto = monto,
                fecha_puja = DateTime.Now
            };

            _context.pujas.Add(nuevaPuja);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Puja realizada exitosamente.";
            return RedirectToAction("Details", "Articulos", new { id = articuloId });
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
