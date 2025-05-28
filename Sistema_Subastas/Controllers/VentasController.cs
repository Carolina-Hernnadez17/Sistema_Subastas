using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_Subastas.Atributo;
using Sistema_Subastas.Models;

namespace Sistema_Subastas.Controllers
{
    [SesionActiva]
    public class VentasController : Controller
    {
        private readonly subastaDbContext _context;

        public VentasController(subastaDbContext context)
        {
            _context = context;
        }

        public IActionResult Historial()
        {
            int? usuarioId = HttpContext.Session.GetInt32("id_usuario");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            var historialVentas = _context.articulos
                .Where(a => a.usuario_id == usuarioId)
                .Select(a => new HistorialVenta
                {
                    Id = a.Id,
                    Nombre = a.titulo,
                    Descripcion = a.descripcion,
                    PrecioFinal = a.precio_venta,
                    FechaCierre = a.fecha_fin,
                    EstadoTransaccion = a.estado_subasta,
                    ImagenUrl = "",
                    Categoria = ""
                })
                .ToList();

            return View("Historial", historialVentas);
        }
        public IActionResult HistorialCompras()
        {
            int? usuarioId = HttpContext.Session.GetInt32("id_usuario");

            if (usuarioId == null)
                return RedirectToAction("Login", "Usuarios");

            // Buscar pujas ganadas por el usuario
            var pujasGanadas = _context.pujas
    .AsEnumerable() // 👈 Esto fuerza la ejecución en memoria (después del GroupBy)
    .GroupBy(p => p.articulo_id)
    .Select(g => g.OrderByDescending(p => p.monto).FirstOrDefault())
    .Where(p => p.usuario_id == usuarioId)
    .Join(_context.articulos,
          puja => puja.articulo_id,
          articulo => articulo.Id,
          (puja, articulo) => new HistorialCompra
          {
              ArticuloId = articulo.Id,
              Titulo = articulo.titulo,
              Descripcion = articulo.descripcion,
              ImagenUrl = "",
              PrecioFinal = articulo.precio_venta,
              FechaCierre = articulo.fecha_fin,
              EstadoTransaccion = articulo.estado_subasta
          })
    .ToList();


            return View("Historial_Compras", pujasGanadas);
        }





    }

}
