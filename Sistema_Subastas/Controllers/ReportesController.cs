﻿using CloudinaryDotNet.Actions;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_Subastas.Models;
using iText.Kernel.Font;
using Newtonsoft.Json;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using System.Linq;
using System.Threading.Tasks;
using Paragraph = iText.Layout.Element.Paragraph;
using iTextSharp.text.pdf;
using PdfFont = iText.Kernel.Font.PdfFont;
using PdfDocument = iText.Kernel.Pdf.PdfDocument;
using PdfWriter = iText.Kernel.Pdf.PdfWriter;

namespace Sistema_Subastas.Controllers
{
    public class ReportesController : Controller
    {
        private readonly subastaDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReportesController(subastaDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult ArticulosMasPopulares()
        {
            // Lógica para obtener datos
            var viewModel = new ReporteArticulosViewModel
            {
                ReportePorPujas = ObtenerReportePorPujas(), // Debe ser una lista de Articulo
                ReportePorVistas = ObtenerReportePorVistas()  // Debe ser una lista de Articulo
            };

            return View(viewModel);
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

            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Colores y fuentes
                Color headerColor = new DeviceRgb(111, 72, 50); // Color del encabezado
                Color textColor = new DeviceRgb(255, 255, 255); // Color del texto
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD); // Fuente negrita

                // Cargar logo
                string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    Image img = new Image(ImageDataFactory.Create(logoPath)).ScaleToFit(100, 100);
                    document.Add(img.SetHorizontalAlignment(HorizontalAlignment.CENTER));
                }

                // Título de la página
                var titleTable = new Table(1).UseAllAvailableWidth();
                titleTable.AddCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Buy-Things").SetFont(boldFont).SetFontSize(20)));
                document.Add(titleTable);

                // Fecha y dirección
                document.Add(new Paragraph($"Fecha de generación: {DateTime.Now:dd/MM/yyyy}")
                    .SetTextAlignment(TextAlignment.CENTER).SetFontSize(12));
                document.Add(new Paragraph("Dirección: Santa Ana | Tu mejor opción en subastas")
                    .SetTextAlignment(TextAlignment.CENTER).SetFontSize(12));
                document.Add(new Paragraph("\n"));

                // Encabezado de sección
                document.Add(new Paragraph("Artículos Más Populares por Pujas")
                    .SetFont(boldFont).SetFontSize(16).SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph("\n"));

                // Tabla de datos
                var table = new Table(5).UseAllAvailableWidth();
                string[] headers = { "Artículo", "Categoría", "Pujas", "Precio Máximo", "Fecha Límite" };
                foreach (var header in headers)
                {
                    table.AddHeaderCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                        .Add(new Paragraph(header).SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                }

                // Rellenar tabla con datos
                foreach (var item in articulosConPujas)
                {
                    table.AddCell(new Cell().Add(new Paragraph(item.titulo).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(item.Categoria).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(item.NumeroDePujas.ToString()).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(item.PrecioMaximo.ToString("C")).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(item.fecha_fin.ToString("dd/MM/yyyy HH:mm")).SetTextAlignment(TextAlignment.CENTER)));
                }

                document.Add(table);
                document.Add(new Paragraph("\n"));

                // Pie de página
                Table footerTable = new Table(1).UseAllAvailableWidth();
                footerTable.AddCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Buy-Things@email.com | (+503) 2490 8943 | El Salvador")));
                document.Add(footerTable);

                document.Close();
                return File(memoryStream.ToArray(), "application/pdf", "Reporte_Articulos_Populares.pdf");
            }
        }


        private List<Articulo> ObtenerReportePorPujas()
        {
            // Obtener datos de la base de datos o cualquier fuente
            return new List<Articulo>(); // Debería devolver una lista de objetos Articulo
        }

        private List<Articulo> ObtenerReportePorVistas()
        {
            // Obtener datos de la base de datos o cualquier fuente
            return new List<Articulo>(); // Debería devolver una lista de objetos Articulo
        }


        public async Task<IActionResult> UsuariosMasActivos()
        {
            var usuariosActivos = await _context.usuarios
                .Select(u => new UsuarioActivoViewModel
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
                .Select(u => new UsuarioActivoViewModel
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

            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                Color headerColor = new DeviceRgb(111, 72, 50); // #6f4832
                Color textColor = new DeviceRgb(255, 255, 255);
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    Image img = new Image(ImageDataFactory.Create(logoPath)).ScaleToFit(100, 100);
                    document.Add(img.SetHorizontalAlignment(HorizontalAlignment.CENTER));
                }

                Table titleTable = new Table(1).UseAllAvailableWidth();
                titleTable.AddCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Buy-Things").SetFont(boldFont).SetFontSize(20)));
                document.Add(titleTable);

                document.Add(new Paragraph($"Fecha de generación: {DateTime.Now:dd/MM/yyyy}")
                    .SetTextAlignment(TextAlignment.CENTER).SetFontSize(12));
                document.Add(new Paragraph("Dirección: Santa Ana | Tu mejor opción en subastas")
                    .SetTextAlignment(TextAlignment.CENTER).SetFontSize(12));
                document.Add(new Paragraph("\n"));

                document.Add(new Paragraph("Usuarios Más Activos")
                    .SetFont(boldFont).SetFontSize(16).SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph("\n"));

                var table = new Table(4).UseAllAvailableWidth();
                table.AddHeaderCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .Add(new Paragraph("Usuario").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .Add(new Paragraph("Artículos").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .Add(new Paragraph("Pujas").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .Add(new Paragraph("Total Gastado").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));

                foreach (var usuario in usuariosActivos)
                {
                    table.AddCell(new Cell().Add(new Paragraph(usuario.Nombre).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(usuario.ArticulosPublicados.ToString()).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(usuario.PujasRealizadas.ToString()).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(usuario.TotalGastado > 0 ? usuario.TotalGastado.ToString("C") : "N/A").SetTextAlignment(TextAlignment.CENTER)));
                }

                document.Add(table);

                document.Add(new Paragraph("\n"));
                Table footerTable = new Table(1).UseAllAvailableWidth();
                footerTable.AddCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Buy-Things@email.com | (+503) 2490 8943 | El Salvador")));
                document.Add(footerTable);

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

                // Definir colores
                Color headerColor = new DeviceRgb(111, 72, 50); // Color #6f4832
                Color textColor = new DeviceRgb(255, 255, 255);
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                // Agregar el logo arriba centrado
                string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    Image img = new Image(ImageDataFactory.Create(logoPath)).ScaleToFit(100, 100);
                    document.Add(img.SetHorizontalAlignment(HorizontalAlignment.CENTER));
                }

                // Franja con el nombre de la empresa
                Table titleTable = new Table(1).UseAllAvailableWidth();
                titleTable.AddCell(new Cell()
                    .SetBackgroundColor(headerColor)
                    .SetFontColor(textColor)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Buy-Things").SetFont(boldFont).SetFontSize(20)));
                document.Add(titleTable);

                // Información general
                document.Add(new Paragraph($"Fecha de generación: {DateTime.Now:dd/MM/yyyy}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12));
                document.Add(new Paragraph("Dirección: Santa Ana | Tu mejor opción en subastas")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12));
                document.Add(new Paragraph("\n"));

                

                
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

                // Pie de página
                document.Add(new Paragraph("\n"));
                Table footerTable = new Table(1).UseAllAvailableWidth();
                footerTable.AddCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Buy-Things@email.com | (+503) 2490 8943 | El Salvador")));
                document.Add(footerTable);

                document.Close();
                return File(memoryStream.ToArray(), "application/pdf", "Reporte_Subastas_Activas.pdf");
            }
        }

        public IActionResult SubastaCerrada()
        {
            var subastas = _context.usuarios
                .Select(u => new
                {
                    Nombre = u.nombre + " " + u.apellido,
                    ArticulosVendidos = _context.articulos.Count(a => a.usuario_id == u.id && a.estado_subasta == "Vendido"),
                    PujasRealizadas = _context.pujas.Count(p => p.usuario_id == u.id),
                    TotalGastado = _context.pujas.Where(p => p.usuario_id == u.id).Sum(p => (decimal?)p.monto) ?? 0
                })
                .Where(s => s.ArticulosVendidos > 0)
                .ToList();

            ViewBag.Subastas = subastas;
            return View();
        }

        public IActionResult DescargarReporteSubastasCerrada()
        {
            var subastas = _context.usuarios
                .Select(u => new
                {
                    Nombre = u.nombre + " " + u.apellido,
                    ArticulosVendidos = _context.articulos.Count(a => a.usuario_id == u.id && a.estado_subasta == "Vendido"),
                    PujasRealizadas = _context.pujas.Count(p => p.usuario_id == u.id),
                    TotalGastado = _context.pujas.Where(p => p.usuario_id == u.id).Sum(p => (decimal?)p.monto) ?? 0
                })
                .Where(s => s.ArticulosVendidos > 0)
                .ToList();

            if (!subastas.Any())
            {
                TempData["Mensaje"] = "No hay subastas cerradas y adjudicadas.";
                return RedirectToAction("SubastasCerradas");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                document.Add(new Paragraph("Reporte de Subastas Cerradas y Adjudicadas")
                    .SetFontSize(18)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                document.Add(new Paragraph("\n"));

                Table table = new Table(4);
                table.AddHeaderCell("Nombre del Usuario");
                table.AddHeaderCell("Artículos Vendidos");
                table.AddHeaderCell("Pujas Realizadas");
                table.AddHeaderCell("Total Gastado en Pujas");

                foreach (var subasta in subastas)
                {
                    table.AddCell(subasta.Nombre);
                    table.AddCell(subasta.ArticulosVendidos.ToString());
                    table.AddCell(subasta.PujasRealizadas.ToString());
                    table.AddCell(subasta.TotalGastado > 0 ? subasta.TotalGastado.ToString("C") : "N/A");
                }

                document.Add(table);
                document.Close();

                return File(ms.ToArray(), "application/pdf", "Reporte_Subastas_Cerradas.pdf");
            }
        }
    }
}

