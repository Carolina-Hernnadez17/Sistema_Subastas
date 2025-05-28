using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Sistema_Subastas.Atributo;
using Sistema_Subastas.Models;
using System.Linq;

namespace Sistema_Subastas.Controllers
{
    [SesionActiva]

    public class EstadisticasController : Controller
    {
        private readonly subastaDbContext _context;

        public EstadisticasController(subastaDbContext context)
        {
            _context = context;
        }

        public IActionResult IngresosPorCategoria()
        {
            var datos = _context.articulos
                .Where(a => a.estado_subasta == "Vendido")
                .Join(_context.articulo_categoria, a => a.Id, ac => ac.articulo_id, (a, ac) => new { a, ac })
                .Join(_context.categorias, ac => ac.ac.categoria_id, c => c.Id, (ac, c) => new { ac.a, Categoria = c.nombre })
                .Join(_context.pujas, a => a.a.Id, p => p.articulo_id, (a, p) => new { a.a.Id, a.Categoria, p.monto })
                .GroupBy(x => x.Categoria)
                .Select(g => new
                {
                    Categoria = g.Key,
                    TotalIngresos = g.Sum(x => x.monto) * 0.90m, // ingreso neto total
                    TotalArticulos = g.Count()
                })
                .ToList();

            ViewBag.Categorias = JsonConvert.SerializeObject(datos.Select(d => d.Categoria));
            ViewBag.Ingresos = JsonConvert.SerializeObject(datos.Select(d => d.TotalIngresos));
            ViewBag.Articulos = JsonConvert.SerializeObject(datos.Select(d => d.TotalArticulos));

            return View("~/Views/Graficas/IngresosPorCategoria.cshtml");
        }
    }
}
