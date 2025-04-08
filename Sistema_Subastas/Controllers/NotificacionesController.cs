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
    public class NotificacionesController : Controller
    {
        private readonly subastaDbContext _context;

        public NotificacionesController(subastaDbContext context)
        {
            _context = context;
        }

        // GET: Notificaciones
        public async Task<IActionResult> Index()
        {
            return View(await _context.notificaciones.ToListAsync());
        }

        // GET: Notificaciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificaciones = await _context.notificaciones
                .FirstOrDefaultAsync(m => m.id == id);
            if (notificaciones == null)
            {
                return NotFound();
            }

            return View(notificaciones);
        }

        // GET: Notificaciones/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Notificaciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,usuario_id,mensaje,leido,fecha")] notificaciones notificaciones)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notificaciones);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(notificaciones);
        }

        // GET: Notificaciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificaciones = await _context.notificaciones.FindAsync(id);
            if (notificaciones == null)
            {
                return NotFound();
            }
            return View(notificaciones);
        }

        // POST: Notificaciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,usuario_id,mensaje,leido,fecha")] notificaciones notificaciones)
        {
            if (id != notificaciones.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notificaciones);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!notificacionesExists(notificaciones.id))
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
            return View(notificaciones);
        }

        // GET: Notificaciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificaciones = await _context.notificaciones
                .FirstOrDefaultAsync(m => m.id == id);
            if (notificaciones == null)
            {
                return NotFound();
            }

            return View(notificaciones);
        }

        // POST: Notificaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notificaciones = await _context.notificaciones.FindAsync(id);
            if (notificaciones != null)
            {
                _context.notificaciones.Remove(notificaciones);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool notificacionesExists(int id)
        {
            return _context.notificaciones.Any(e => e.id == id);
        }
        public IActionResult Notificacion()
        {
            int? usuarioId = HttpContext.Session.GetInt32("id_usuario");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }
            //int usuarioId = (int)HttpContext.Session.GetInt32("usuario_id");
            var notis = _context.notificaciones
                .Where(n => n.usuario_id == usuarioId)
                .OrderByDescending(n => n.fecha)
                .ToList();

            return View(notis);
        }

    }
}
