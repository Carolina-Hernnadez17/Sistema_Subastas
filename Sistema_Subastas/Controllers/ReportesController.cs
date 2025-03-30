using CloudinaryDotNet.Actions;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_Subastas.Models;
using System.Linq;
using System.Threading.Tasks;
using Paragraph = iText.Layout.Element.Paragraph;

namespace Sistema_Subastas.Controllers
{
    public class ReportesController : Controller
    {
        private readonly subastaDbContext _context;

        public ReportesController(subastaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ArticulosMasPopulares()
        {
            // Reporte por Pujas
            var articulosConPujas = await _context.articulos
                .Join(_context.articulo_categoria, a => a.Id, ac => ac.articulo_id, (a, ac) => new { a, ac })
                .Join(_context.categorias, ac => ac.ac.categoria_id, c => c.Id, (ac, c) => new { ac.a, Categoria = c.nombre })
                .Join(_context.pujas, a => a.a.Id, p => p.articulo_id, (a, p) => new { a.a, a.Categoria, p })
                .GroupBy(a => new { a.a.Id, a.a.titulo, a.Categoria, a.a.fecha_fin })
                .Select(g => new
                {
                    g.Key.titulo,
                    g.Key.Categoria,
                    NumeroDePujas = g.Count(),
                    PrecioMaximo = g.Max(p => p.p.monto),
                    g.Key.fecha_fin
                })
                .OrderByDescending(a => a.NumeroDePujas)
                .ToListAsync();

            // Reporte por Visualizaciones
            var articulosConVisualizaciones = await _context.articulos
                .Join(_context.articulo_categoria, a => a.Id, ac => ac.articulo_id, (a, ac) => new { a, ac })
                .Join(_context.categorias, ac => ac.ac.categoria_id, c => c.Id, (ac, c) => new { ac.a, Categoria = c.nombre })
                .Join(_context.seguimiento_subastas, a => a.a.Id, s => s.articulo_id, (a, s) => new { a.a, a.Categoria, s })
                .GroupBy(a => new { a.a.Id, a.a.titulo, a.Categoria, a.a.fecha_fin })
                .Select(g => new
                {
                    g.Key.titulo,
                    g.Key.Categoria,
                    NumeroDeVisualizaciones = g.Count(),
                    g.Key.fecha_fin
                })
                .OrderByDescending(a => a.NumeroDeVisualizaciones)
                .ToListAsync();

            var model = new
            {
                ReportePorPujas = articulosConPujas,
                ReportePorVisualizaciones = articulosConVisualizaciones
            };

            return View(model);
        }

        public IActionResult DescargarReportePdf()
        {
            var articulosConPujas = _context.articulos
                .Join(_context.articulo_categoria, a => a.Id, ac => ac.articulo_id, (a, ac) => new { a, ac })
                .Join(_context.categorias, ac => ac.ac.categoria_id, c => c.Id, (ac, c) => new { ac.a, Categoria = c.nombre })
                .Join(_context.pujas, a => a.a.Id, p => p.articulo_id, (a, p) => new { a.a, a.Categoria, p })
                .GroupBy(a => new { a.a.Id, a.a.titulo, a.Categoria, a.a.fecha_fin })
                .Select(g => new
                {
                    g.Key.titulo,
                    g.Key.Categoria,
                    NumeroDePujas = g.Count(),
                    PrecioMaximo = g.Max(p => p.p.monto),
                    g.Key.fecha_fin
                })
                .OrderByDescending(a => a.NumeroDePujas)
                .ToList();

            // Crear documento PDF
            using (var memoryStream = new System.IO.MemoryStream())
            {
                var pdfWriter = new PdfWriter(memoryStream);
                var pdfDocument = new PdfDocument(pdfWriter);
                var document = new Document(pdfDocument);

                document.Add(new Paragraph("Reporte de Artículos Más Populares"));
                document.Add(new Paragraph(" "));

                // Crear tabla
                var table = new Table(5);

                // Añadir encabezados
                table.AddHeaderCell("Nombre del Artículo");
                table.AddHeaderCell("Categoría");
                table.AddHeaderCell("Número de Pujas");
                table.AddHeaderCell("Precio Actual Más Alto");
                table.AddHeaderCell("Fecha Límite de Subasta");

                // Añadir filas
                foreach (var item in articulosConPujas)
                {
                    table.AddCell(item.titulo);
                    table.AddCell(item.Categoria);
                    table.AddCell(item.NumeroDePujas.ToString());
                    table.AddCell(item.PrecioMaximo.ToString("C"));
                    table.AddCell(item.fecha_fin.ToString("dd/MM/yyyy HH:mm"));
                }

                document.Add(table);

                document.Close();
                return File(memoryStream.ToArray(), "application/pdf", "Reporte_Articulos_Populares.pdf");
            }
        }
    }
}
