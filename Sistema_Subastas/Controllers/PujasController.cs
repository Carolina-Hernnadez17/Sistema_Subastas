
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
            // Obtener el artículo
            var articulo = _context.articulos.FirstOrDefault(a => a.Id == id);
            if (articulo == null)
            {
                return NotFound();
            }

            // Cargar imágenes asociadas al artículo (suponiendo que la tabla se llama "imagenes_articulos")
            var imagenes = _context.imagenes_articulos
                           .Where(i => i.articulo_id == id)
                           .ToList();

            // Realizar join entre pujas y usuarios para obtener historial
            var historialPujas = (from p in _context.pujas
                                  join u in _context.usuarios on p.usuario_id equals u.id
                                  where p.articulo_id == id
                                  orderby p.fecha_puja descending
                                  select new
                                  {
                                      id_puja = p.Id,
                                      Valor = p.monto,       // Usamos "Valor" con V mayúscula
                                      Fecha = p.fecha_puja,       // "Fecha" con F mayúscula
                                      id_usuario = p.usuario_id,
                                      nombre_usuario = u.nombre
                                  }).ToList<dynamic>();

            // Obtener el id del usuario logueado desde la sesión (asegúrate de haberlo guardado al iniciar sesión)
            int? usuarioId = HttpContext.Session.GetInt32("usuario_id");

            // Pasar los datos a la vista mediante ViewBag
            ViewBag.Articulos = articulo;
            ViewBag.Imagenes = imagenes;
            ViewBag.HistorialPujas = historialPujas;
            ViewBag.UsuarioId = usuarioId;
            // También pasamos el estado de la subasta
            ViewBag.EstadoSubasta = articulo.estado; // o puedes usar otro campo si manejas "activa" / "finalizada"

            return View();
        }

        // Acción para cancelar una puja (POST)
        [HttpPost]
        public IActionResult CancelarPuja(int id)
        {
            int? usuarioId = HttpContext.Session.GetInt32("usuario_id");
            if (usuarioId == null)
                return RedirectToAction("Login", "Usuarios"); // O redirigir según tu lógica

            // Buscar la puja a cancelar
            var puja = _context.pujas.FirstOrDefault(p => p.Id == id);
            if (puja == null)
                return NotFound();

            // Verificar que la puja sea del usuario logueado
            if (puja.usuario_id != usuarioId)
            {
                TempData["Error"] = "No tienes permiso para cancelar esta puja.";
                return RedirectToAction("VerPujas", new { id = puja.articulo_id });
            }

            // Verificar que la subasta esté activa (asumiendo que el estado "activa" indica que se puede cancelar)
            var articulo = _context.articulos.FirstOrDefault(a => a.Id == puja.articulo_id);
            if (articulo == null || articulo.estado != "activa")
            {
                TempData["Error"] = "La puja no se puede cancelar porque la subasta ya finalizó.";
                return RedirectToAction("VerPujas", new { id = puja.articulo_id });
            }

            // Eliminar la puja
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
