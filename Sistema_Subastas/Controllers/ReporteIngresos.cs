using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Sistema_Subastas.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Paragraph = iText.Layout.Element.Paragraph;
using Newtonsoft.Json;


namespace Sistema_Subastas.Controllers
{
    public class ReporteIngresos : Controller
    {
        private readonly subastaDbContext _context;

        public ReporteIngresos(subastaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> VerIngresos(DateTime? fechaInicio, DateTime? fechaFin)
        {
            int? vendedorId = HttpContext.Session.GetInt32("id_usuario");

            if (vendedorId == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            var query = _context.articulos
                .Where(a => a.usuario_id == vendedorId && a.estado_subasta == "Vendido");

            if (fechaInicio.HasValue)
                query = query.Where(a => a.fecha_fin >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(a => a.fecha_fin <= fechaFin.Value);

            var ingresos = await query
                .Join(_context.pujas,
                    articulo => articulo.Id,
                    puja => puja.articulo_id,
                    (articulo, puja) => new { articulo, puja })
                .GroupBy(g => g.articulo.Id)
                .Select(g => new Ingresos
                {
                    Titulo = g.First().articulo.titulo,
                    FechaVenta = g.First().articulo.fecha_fin,
                    PrecioFinal = g.Max(x => x.puja.monto),
                    Comision = g.Max(x => x.puja.monto) * 0.10m,
                    IngresoNeto = g.Max(x => x.puja.monto) * 0.90m
                })
                .OrderByDescending(x => x.FechaVenta)
                .ToListAsync();

            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");

            return View("~/Views/Reportes/ReporteIngresos.cshtml", ingresos);
        }

        public IActionResult DescargarReporteIngresos(DateTime? fechaInicio, DateTime? fechaFin)
        {
            int? vendedorId = HttpContext.Session.GetInt32("id_usuario");

            if (vendedorId == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            var query = _context.articulos
                .Where(a => a.usuario_id == vendedorId && a.estado_subasta == "Vendido");

            if (fechaInicio.HasValue)
                query = query.Where(a => a.fecha_fin >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(a => a.fecha_fin <= fechaFin.Value);

            var ingresos = query
                .Join(_context.pujas,
                    articulo => articulo.Id,
                    puja => puja.articulo_id,
                    (articulo, puja) => new { articulo, puja })
                .GroupBy(g => g.articulo.Id)
                .Select(g => new Ingresos
                {
                    Titulo = g.First().articulo.titulo,
                    FechaVenta = g.First().articulo.fecha_fin,
                    PrecioFinal = g.Max(x => x.puja.monto),
                    Comision = g.Max(x => x.puja.monto) * 0.10m,
                    IngresoNeto = g.Max(x => x.puja.monto) * 0.90m
                })
                .OrderByDescending(x => x.FechaVenta)
                .ToList();

            using (var ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                document.Add(new Paragraph("Reporte de Ingresos por Subastas")
                    .SetFontSize(16));

                document.Add(new Paragraph("Fecha de generación: " + TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/El_Salvador").ToString("dd/MM/yyyy HH:mm")));

                document.Add(new Paragraph("\n"));

                var table = new Table(5);
                table.AddHeaderCell("Artículo");
                table.AddHeaderCell("Fecha Venta");
                table.AddHeaderCell("Precio Final");
                table.AddHeaderCell("Comisión (10%)");
                table.AddHeaderCell("Ingreso Neto");

                foreach (var item in ingresos)
                {
                    table.AddCell(item.Titulo);
                    table.AddCell(item.FechaVenta.ToString("dd/MM/yyyy HH:mm"));
                    table.AddCell(item.PrecioFinal.ToString("C"));
                    table.AddCell(item.Comision.ToString("C"));
                    table.AddCell(item.IngresoNeto.ToString("C"));
                }

                document.Add(table);
                document.Close();

                return File(ms.ToArray(), "application/pdf", "Reporte_Ingresos_Subastas.pdf");
            }
        }

        public IActionResult GraficoEvolucionPujas(int articuloId)
        {
            var datos = _context.pujas
            .Where(p => p.articulo_id == articuloId)
            .GroupBy(p => p.fecha_puja.Date)
            .AsEnumerable() // 👈 Esto trae los datos a memoria y permite usar ToString()
            .Select(g => new
            {
                Fecha = g.Key.ToString("yyyy-MM-dd"),
                TotalPujas = g.Count()
            })
            .OrderBy(g => g.Fecha)
            .ToList();


            var categorias = datos.Select(d => d.Fecha).ToList();
            var valores = datos.Select(d => d.TotalPujas).ToList();

            ViewBag.Categorias = JsonConvert.SerializeObject(categorias);
            ViewBag.Valores = JsonConvert.SerializeObject(valores);
            ViewBag.NombreArticulo = _context.articulos.Where(a => a.Id == articuloId).Select(a => a.titulo).FirstOrDefault();

            return View("~/Views/Graficas/GraficoEvolucion.cshtml");
        }



    }




}
