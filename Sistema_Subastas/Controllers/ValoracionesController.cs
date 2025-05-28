using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema_Subastas.Atributo;
using Sistema_Subastas.Models;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Sistema_Subastas.Controllers
{
    [SesionActiva]

    public class ValoracionesController : Controller
    {
        private readonly subastaDbContext _context;

        public ValoracionesController(subastaDbContext context)
        {
            _context = context;
        }

        // GET: Valoraciones
        public async Task<IActionResult> Index()
        {
            return View(await _context.valoraciones.ToListAsync());
        }

        // GET: Valoraciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {

           


            if (id == null)
            {
                return NotFound();
            }

            var valoraciones = await _context.valoraciones
                .FirstOrDefaultAsync(m => m.id == id);
            if (valoraciones == null)
            {
                return NotFound();
            }

            return View(valoraciones);
        }

        // GET: Valoraciones/Create
        public IActionResult Create(int id)
        {


            var consulta_vendedor = _context.articulos.Where(a => a.Id == id).FirstOrDefault();

            
            ViewBag.VendedorValorado = consulta_vendedor.usuario_id;

            int? usuarioId = HttpContext.Session.GetInt32("id_usuario");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }
            ViewBag.UsuarioQueValora = usuarioId;

            return View();
        }

        // POST: Valoraciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,usuario_valorado_id,usuario_que_valora_id,puntuacion,comentario,fecha")] valoraciones valoraciones)
        {

            valoraciones.id= 0; 

            if (ModelState.IsValid)
            {
                _context.Add(valoraciones);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Imagenes_articulos");
            }
            return View(valoraciones);
        }

        // GET: Valoraciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var valoraciones = await _context.valoraciones.FindAsync(id);
            if (valoraciones == null)
            {
                return NotFound();
            }
            return View(valoraciones);
        }

        // POST: Valoraciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,usuario_valorado_id,usuario_que_valora_id,puntuacion,comentario,fceha")] valoraciones valoraciones)
        {
            if (id != valoraciones.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(valoraciones);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!valoracionesExists(valoraciones.id))
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
            return View(valoraciones);
        }

        // GET: Valoraciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var valoraciones = await _context.valoraciones
                .FirstOrDefaultAsync(m => m.id == id);
            if (valoraciones == null)
            {
                return NotFound();
            }

            return View(valoraciones);
        }

        // POST: Valoraciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var valoraciones = await _context.valoraciones.FindAsync(id);
            if (valoraciones != null)
            {
                _context.valoraciones.Remove(valoraciones);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool valoracionesExists(int id)
        {
            return _context.valoraciones.Any(e => e.id == id);
        }
    }
}
