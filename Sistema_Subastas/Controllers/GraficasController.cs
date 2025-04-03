using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_Subastas.Models;
using Newtonsoft.Json;

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


    }
}
