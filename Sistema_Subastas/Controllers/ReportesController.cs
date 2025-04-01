using CloudinaryDotNet.Actions;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
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


        public async Task<IActionResult> UsuariosMasActivos()
        {
            var usuariosActivos = await _context.usuarios
                .Select(u => new
                {
                    Nombre = u.nombre,
                    ArticulosPublicados = _context.articulos.Count(a => a.usuario_id == u.id),
                    PujasRealizadas = _context.pujas.Count(p => p.usuario_id == u.id),
                    TotalGastado = _context.pujas
                        .Where(p => p.usuario_id == u.id)
                        .Sum(p => (decimal?)p.monto) ?? 0
                })
                .OrderByDescending(u => u.PujasRealizadas + u.ArticulosPublicados)
                .ToListAsync();

            return View(usuariosActivos);
        }

        public IActionResult DescargarReporteUsuarios()
        {
            var usuariosActivos = _context.usuarios
                .Select(u => new
                {
                    Nombre = u.nombre,
                    ArticulosPublicados = _context.articulos.Count(a => a.usuario_id == u.id),
                    PujasRealizadas = _context.pujas.Count(p => p.usuario_id == u.id),
                    TotalGastado = _context.pujas
                        .Where(p => p.usuario_id == u.id)
                        .Sum(p => (decimal?)p.monto) ?? 0
                })
                .OrderByDescending(u => u.PujasRealizadas + u.ArticulosPublicados)
                .ToList();

            using (var memoryStream = new System.IO.MemoryStream())
            {
                var pdfWriter = new PdfWriter(memoryStream);
                var pdfDocument = new PdfDocument(pdfWriter);
                var document = new Document(pdfDocument);

                // Fuente en negrita para el título
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                // Título del reporte
                document.Add(new Paragraph("Reporte de Usuarios Más Activos")
                    .SetFont(boldFont)
                    .SetFontSize(14));

                document.Add(new Paragraph(" "));

                // Tabla con 4 columnas
                var table = new Table(4);
                table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre del Usuario").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Artículos Publicados").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Pujas Realizadas").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Total Gastado en Pujas").SetFont(boldFont)));

                // Agregar los datos
                foreach (var usuario in usuariosActivos)
                {
                    table.AddCell(new Cell().Add(new Paragraph(usuario.Nombre)));
                    table.AddCell(new Cell().Add(new Paragraph(usuario.ArticulosPublicados.ToString())));
                    table.AddCell(new Cell().Add(new Paragraph(usuario.PujasRealizadas.ToString())));

                    if (usuario.TotalGastado > 0)
                        table.AddCell(new Cell().Add(new Paragraph(usuario.TotalGastado.ToString("C"))));
                    else
                        table.AddCell(new Cell().Add(new Paragraph("N/A")));
                }

                document.Add(table);
                document.Close();

                return File(memoryStream.ToArray(), "application/pdf", "Reporte_Usuarios_Activos.pdf");
            }
        }

        public async Task<IActionResult> SubastasActivas()
        {
            var fechaActual = DateTime.Now;

            var subastasActivas = await _context.articulos
                .Where(a => a.estado_subasta == "Publicado" && a.fecha_fin > fechaActual)
                .Select(a => new
                {
                    id = a.Id,
                    Titulo = a.titulo,
                    PrecioSalida = a.precio_salida,
                    FechaCierre = a.fecha_fin,
                    NumeroPujas = _context.pujas.Count(p => p.articulo_id == a.Id)
                })
                .OrderBy(a => a.FechaCierre)
                .ToListAsync();

            return View(subastasActivas);
        }

        // GET: 
        public IActionResult DescargarReporteSubastas()
        {
            var fechaActual = DateTime.Now;

            var subastasActivas = _context.articulos
                .Where(a => a.estado_subasta == "Publicado" && a.fecha_fin > fechaActual)
                .Select(a => new
                {
                    id = a.Id,
                    Titulo = a.titulo,
                    PrecioSalida = a.precio_salida,
                    FechaCierre = a.fecha_fin,
                    NumeroPujas = _context.pujas.Count(p => p.articulo_id == a.Id)
                })
                .OrderBy(a => a.FechaCierre)
                .ToList();

            
            using (var memoryStream = new MemoryStream())
            {
                var pdfWriter = new PdfWriter(memoryStream);
                var pdfDocument = new PdfDocument(pdfWriter);
                var document = new Document(pdfDocument);

                
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                
                document.Add(new Paragraph("Reporte de Subastas Activas")
                    .SetFont(boldFont)
                    .SetFontSize(14));

                document.Add(new Paragraph("Fecha de generación: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                    .SetFontSize(10));

                document.Add(new Paragraph(" "));

                
                var table = new Table(4);

                
                table.AddHeaderCell(new Cell().Add(new Paragraph("Título de la subasta").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Precio inicial").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha de cierre").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Número de pujas").SetFont(boldFont)));

                
                foreach (var item in subastasActivas)
                {
                    table.AddCell(new Cell().Add(new Paragraph(item.Titulo)));
                    table.AddCell(new Cell().Add(new Paragraph(item.PrecioSalida.ToString("C"))));
                    table.AddCell(new Cell().Add(new Paragraph(item.FechaCierre.ToString("dd/MM/yyyy HH:mm"))));
                    table.AddCell(new Cell().Add(new Paragraph(item.NumeroPujas.ToString())));
                }

                document.Add(table);
                              
                document.Close();
                return File(memoryStream.ToArray(), "application/pdf", "Reporte_Subastas_Activas.pdf");
            }
        }
    }
}

