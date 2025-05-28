using CloudinaryDotNet.Actions;
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
using Sistema_Subastas.Atributo;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using Rotativa;

namespace Sistema_Subastas.Controllers
{
    [SesionActiva]

    public class ReportesController : Controller
    {
        private readonly subastaDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReportesController(subastaDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult ArticulosPopulares()
        {
            var modelo = _context.articulos
                .Select(a => new ReporteArticulosViewModel
                {
                    Titulo = a.titulo,
                    // Contar las pujas relacionadas con el artículo
                    NumeroPujas = _context.pujas.Count(p => p.articulo_id == a.Id),
                    PrecioSalida = a.precio_salida,
                    FechaInicio = a.fecha_inicio,
                    FechaFin = a.fecha_fin
                })
                .OrderByDescending(x => x.NumeroPujas)
                .ToList();

            return View(modelo);
        }



        public IActionResult DescargarArticulosPopulares()
        {
            var articulosPopulares = _context.articulos
                .Join(_context.articulo_categoria, a => a.Id, ac => ac.articulo_id, (a, ac) => new { a, ac })
                .Join(_context.categorias, ac => ac.ac.categoria_id, c => c.Id, (ac, c) => new { ac.a, Categoria = c.nombre })
                .GroupJoin(_context.pujas, a => a.a.Id, p => p.articulo_id, (a, pujas) => new { a.a, a.Categoria, Pujas = pujas })
                .Select(g => new
                {
                    g.a.titulo,
                    g.Categoria,
                    NumeroDePujas = g.Pujas.Count(),
                    PrecioMaximo = g.Pujas.Any() ? g.Pujas.Max(p => p.monto) : 0,
                    g.a.fecha_fin
                })
                .OrderByDescending(a => a.NumeroDePujas)
                .ToList();

            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Colores y fuentes
                Color headerColor = new DeviceRgb(111, 72, 50); // Café
                Color textColor = new DeviceRgb(255, 255, 255); // Blanco
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                // Logo
                string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    Image img = new Image(ImageDataFactory.Create(logoPath)).ScaleToFit(100, 100);
                    document.Add(img.SetHorizontalAlignment(HorizontalAlignment.CENTER));
                }

                // Título principal
                var titleTable = new Table(1).UseAllAvailableWidth();
                titleTable.AddCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Buy-Things").SetFont(boldFont).SetFontSize(20)));
                document.Add(titleTable);

                // Fecha y ubicación
                document.Add(new Paragraph($"Fecha de generación: {TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/El_Salvador"):dd/MM/yyyy}")
                    .SetTextAlignment(TextAlignment.CENTER).SetFontSize(12));
                document.Add(new Paragraph("Dirección: Santa Ana | Tu mejor opción en subastas")
                    .SetTextAlignment(TextAlignment.CENTER).SetFontSize(12));
                document.Add(new Paragraph("\n"));

                // Encabezado sección
                document.Add(new Paragraph("Artículos Más Populares")
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

                foreach (var item in articulosPopulares)
                {
                    table.AddCell(new Cell().Add(new Paragraph(item.titulo).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(item.Categoria).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(item.NumeroDePujas.ToString()).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(item.PrecioMaximo.ToString("C")).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(item.fecha_fin.ToString("dd/MM/yyyy HH:mm")).SetTextAlignment(TextAlignment.CENTER)));
                }

                document.Add(table);
                document.Add(new Paragraph("\n"));

                // Footer
                Table footerTable = new Table(1).UseAllAvailableWidth();
                footerTable.AddCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Buy-Things@email.com | (+503) 2490 8943 | El Salvador")));
                document.Add(footerTable);

                document.Close();
                return File(memoryStream.ToArray(), "application/pdf", "Reporte_Articulos_Populares.pdf");
            }
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
                document.Add(new Paragraph($"Fecha de generación: {TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/El_Salvador"):dd/MM/yyyy}")
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

                document.Add(new Paragraph($"Fecha de generación: {TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/El_Salvador"):dd/MM/yyyy}")
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
            var fechaActual = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/El_Salvador");

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
            var fechaActual = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/El_Salvador");

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
                document.Add(new Paragraph($"Fecha de generación: {TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/El_Salvador"):dd/MM/yyyy}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12));
                document.Add(new Paragraph("Dirección: Santa Ana | Tu mejor opción en subastas")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12));
                document.Add(new Paragraph("\n"));

                

                
                document.Add(new Paragraph("Reporte de Subastas Activas")
                    .SetFont(boldFont)
                    .SetFontSize(14));

                document.Add(new Paragraph("Fecha de generación: " + TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/El_Salvador").ToString("dd/MM/yyyy HH:mm"))
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

        public IActionResult SubastaCerrada(int id)
        {
            var subastas = _context.articulos
                .Where(a => a.usuario_id == id && a.estado_subasta == "Vendido")
                .Select(a => new
                {
                    Titulo = a.titulo,
                    PrecioVenta = a.precio_venta,
                    FechaCierre = a.fecha_fin,
                    NumeroPujas = _context.pujas.Count(p => p.articulo_id == a.Id)
                })
                .ToList();

            ViewBag.TotalPrecioPujas = subastas.Sum(s => s.PrecioVenta);
            ViewBag.CantidadArticulos = subastas.Count;

            return View(subastas); // Pasa solo la lista como Model
        }


        public IActionResult DescargarReporteSubastasCerrada(int id)
        {
            var usuario = _context.usuarios.FirstOrDefault(u => u.id == id);
            if (usuario == null)
            {
                
                return RedirectToAction("SubastaCerrada", new { id });
            }

            var subastas = _context.articulos
                .Where(a => a.usuario_id == id && a.estado_subasta == "Vendido")
                .Select(a => new
                {
                    Titulo = a.titulo,
                    PrecioVenta = a.precio_venta,
                    FechaCierre = a.fecha_fin,
                    NumeroPujas = _context.pujas.Count(p => p.articulo_id == a.Id)
                })
                .ToList();

            if (!subastas.Any())
            {
                TempData["Mensaje"] = "No hay subastas cerradas y adjudicadas para este usuario.";
                return RedirectToAction("SubastaCerrada", new { id });
            }

            var totalPrecioPujas = subastas.Sum(s => s.PrecioVenta);
            var cantidadArticulos = subastas.Count;

            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                // Fuente negrita (Helvetica-Bold)
                PdfFont fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                document.Add(new Paragraph("Reporte de Subastas Cerradas y Adjudicadas")
                    .SetFont(fontBold)
                    .SetFontSize(18)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                document.Add(new Paragraph($"\nUsuario: {usuario.nombre} {usuario.apellido}").SetFont(fontNormal));
                document.Add(new Paragraph($"Fecha del reporte: {DateTime.Now:yyyy-MM-dd}\n").SetFont(fontNormal));

                // Tabla con los detalles de cada subasta
                Table table = new Table(4);
                table.AddHeaderCell(new Cell().Add(new Paragraph("Título del Artículo").SetFont(fontBold)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Precio de Venta").SetFont(fontBold)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha de Cierre").SetFont(fontBold)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Nº de Pujas").SetFont(fontBold)));

                foreach (var s in subastas)
                {
                    table.AddCell(new Cell().Add(new Paragraph(s.Titulo).SetFont(fontNormal)));
                    table.AddCell(new Cell().Add(new Paragraph(s.PrecioVenta.ToString("C")).SetFont(fontNormal)));
                    table.AddCell(new Cell().Add(new Paragraph(s.FechaCierre.ToString("yyyy-MM-dd")).SetFont(fontNormal)));
                    table.AddCell(new Cell().Add(new Paragraph(s.NumeroPujas.ToString()).SetFont(fontNormal)));
                }

                document.Add(table);

                // Resumen
                document.Add(new Paragraph("\nResumen")
                    .SetFont(fontBold)
                    .SetFontSize(14)
                    .SetMarginTop(20));

                document.Add(new Paragraph($"Total recaudado: {totalPrecioPujas.ToString("C")}").SetFont(fontNormal));
                document.Add(new Paragraph($"Cantidad de artículos vendidos: {cantidadArticulos}").SetFont(fontNormal));

                document.Close();

                return File(ms.ToArray(), "application/pdf", "Reporte_Subastas_Cerradas.pdf");
            }
        }

    }
}

