using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_Subastas.Models;
using Newtonsoft.Json;
using iText.Commons.Actions.Contexts;

namespace Sistema_Subastas.Controllers
{
    public class GraficasController : Controller
    {
        private readonly subastaDbContext _subastaDbContext;

        public GraficasController(subastaDbContext subastaDbContext)
        {
            _subastaDbContext = subastaDbContext;
        }

        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> ResumenUsuarios()
        {
            var subastas_fechaa = _subastaDbContext.usuarios.ToList();


            var meses = Enumerable.Range(1, 12).ToDictionary(m => m, m => 0);


            var group = subastas_fechaa
                        .Where(u => u.fecha_registro != null)
                        .GroupBy(u => u.fecha_registro.Month)
                        .Select(g => new
                        {
                            Mes = g.Key,
                            Usuarios = g.Count()
                        })
                        .ToList();


            foreach (var item in group)
            {
                meses[item.Mes] = item.Usuarios;
            }


            var nombresMeses = new string[]
            {
                "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
            };

            var datosGrafica = new
            {
                series = new[]
                {
                    new
                    {
                        name = "Registro de Usuarios",
                        colorByPoint = true,
                        data = meses.Select(m => new
                        {
                            name = nombresMeses[m.Key - 1],
                            y = m.Value
                        }).ToArray()
                    }
                }
            };


            ViewBag.DatosGrafica = JsonConvert.SerializeObject(datosGrafica);

            return View();
        }


        public async Task<IActionResult> ArticulosMasPopulares()
        {
            var articulosMasPujados = await _subastaDbContext.pujas
                .GroupBy(p => p.articulo_id)
                .Select(g => new
                {
                    Articulo = _subastaDbContext.articulos.Where(a => a.Id == g.Key).Select(a => a.titulo).FirstOrDefault(),
                    Pujas = g.Count()
                })
                .OrderByDescending(a => a.Pujas)
                .Take(10)
                .ToListAsync();

            var datosGrafica = new
            {
                series = new[]
                {
                new
                {
                    name = "Número de Pujas",
                    colorByPoint = true,
                    data = articulosMasPujados.Select(a => new
                    {
                        name = a.Articulo,
                        y = a.Pujas
                    }).ToArray()
                }
            }
            };

            ViewBag.DatosGrafica = JsonConvert.SerializeObject(datosGrafica);
            return View();
        }

        public async Task<IActionResult> DistribucionCategorias()
        {
            var categoriasData = await _subastaDbContext.articulo_categoria
                .GroupBy(ac => ac.categoria_id)
                .Select(g => new
                {
                    Categoria = _subastaDbContext.categorias.Where(c => c.Id == g.Key).Select(c => c.nombre).FirstOrDefault(),
                    Cantidad = g.Count()
                })
                .ToListAsync();

            var totalArticulos = categoriasData.Sum(c => c.Cantidad);

            var datosGrafica = new
            {
                series = new[]
                {
                new
                {
                    name = "Distribución de Categorías",
                    colorByPoint = true,
                    data = categoriasData.Select(c => new
                    {
                        name = c.Categoria,
                        y = (double)c.Cantidad / totalArticulos * 100
                    }).ToArray()
                }
            }
            };

            ViewBag.DatosGrafica = JsonConvert.SerializeObject(datosGrafica);
            return View();
        }


    }
}
