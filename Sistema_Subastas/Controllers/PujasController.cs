
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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


        // GET: Pujas/Realizar/5
        public async Task<IActionResult> Realizar(int id)
        {
            var articulo = await _context.articulos
                .FirstOrDefaultAsync(a => a.Id == id);

            if (articulo == null || articulo.estado_subasta != "Publicado" || articulo.fecha_fin < DateTime.Now)
            {
                return NotFound("Artículo no disponible para pujas.");
            }

            var historial = await (from p in _context.pujas
                                   join u in _context.usuarios on p.usuario_id equals u.id
                                   where p.articulo_id == id
                                   orderby p.monto descending
                                   select new PujaHistorialViewModel
                                   {
                                       NombreUsuario = u.nombre,
                                       Monto = p.monto,
                                       Fecha = p.fecha_puja
                                   }).ToListAsync();

            ViewBag.Articulo = articulo;
            ViewBag.Historial = historial;

            return View();
        }

        // Acción para registrar la puja
        [HttpPost]
        public IActionResult RegistrarPuja(int ArticuloId, int UsuarioId, decimal Monto)
        {
            var articulo = _context.articulos.FirstOrDefault(a => a.Id == ArticuloId);
            var usuario = _context.usuarios.FirstOrDefault(u => u.id == UsuarioId);

            if (articulo == null || usuario == null)
            {
                return NotFound("El artículo o el usuario no existen.");
            }

            if (articulo.estado_subasta != "Publicado" || articulo.fecha_fin < DateTime.Now)
            {
                return BadRequest("La subasta ya ha finalizado o no está disponible.");
            }

            if (Monto <= articulo.precio_salida)
            {
                return BadRequest("El monto de la puja debe ser mayor al precio de salida.");
            }

            var puja = new pujas
            {
                articulo_id = ArticuloId,
                usuario_id = UsuarioId,
                monto = Monto,
                fecha_puja = DateTime.Now
            };
            var notificacion = new notificaciones
            {
                usuario_id = articulo.usuario_id,
                mensaje = $"📢 Nueva puja de ${puja.monto} en tu subasta: {articulo.titulo}",
                leido = false,
                fecha = DateTime.Now
            };

            _context.pujas.Add(puja);
            _context.notificaciones.Add(notificacion);

            _context.SaveChanges();

            return RedirectToAction("Details", "Imagenes_articulos", new { id = ArticuloId });

        }

        // Acción para mostrar el detalle del artículo, imágenes y el historial de pujas
        public IActionResult VerPujas(int id)
        {
            var articulo = _context.articulos.FirstOrDefault(a => a.Id == id);
            if (articulo == null)
            {
                return NotFound();
            }

            var imagenes = _context.imagenes_articulos
                           .Where(i => i.articulo_id == id)
                           .ToList();

            var historialPujas = (from p in _context.pujas
                                  join u in _context.usuarios on p.usuario_id equals u.id
                                  where p.articulo_id == id
                                  orderby p.fecha_puja descending
                                  select new
                                  {
                                      id_puja = p.Id,
                                      Valor = p.monto,        
                                      Fecha = p.fecha_puja,     
                                      id_usuario = p.usuario_id,
                                      nombre_usuario = u.nombre
                                  }).ToList<dynamic>();

            int? usuarioId = HttpContext.Session.GetInt32("usuario_id");

            ViewBag.Articulos = articulo;
            ViewBag.Imagenes = imagenes;
            ViewBag.HistorialPujas = historialPujas;
            ViewBag.UsuarioId = usuarioId;
            ViewBag.EstadoSubasta = articulo.estado_subasta;

            return View();
        }

        [HttpPost]
        public IActionResult CancelarPuja(int id)
        {
            int? usuarioId = HttpContext.Session.GetInt32("usuario_id");
            if (usuarioId == null)
                return RedirectToAction("Login", "Usuarios");

            var puja = _context.pujas.FirstOrDefault(p => p.Id == id);
            if (puja == null)
                return NotFound();

            if (puja.usuario_id != usuarioId)
            {
                TempData["Error"] = "No tienes permiso para cancelar esta puja.";
                return RedirectToAction("VerPujas", new { id = puja.articulo_id });
            }

            var articulo = _context.articulos.FirstOrDefault(a => a.Id == puja.articulo_id);
            if (articulo == null || articulo.estado_subasta != "Publicado")
            {
                TempData["Error"] = "La puja no se puede cancelar porque la subasta ya finalizó.";
                return RedirectToAction("VerPujas", new { id = puja.articulo_id });
            }

            _context.pujas.Remove(puja);
            _context.SaveChanges();

            TempData["Exito"] = "Puja cancelada con éxito.";
            return RedirectToAction("VerPujas", new { id = puja.articulo_id });
        }

        // GET: Pujas
        public async Task<IActionResult> Index()
        {
            return View(await _context.pujas.ToListAsync());
        }


    }

    public class PujaHistorialViewModel
    {
        public string NombreUsuario { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
    }

}
