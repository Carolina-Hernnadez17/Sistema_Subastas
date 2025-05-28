using iText.IO.Image;
using Microsoft.AspNetCore.Mvc;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Sistema_Subastas.Models;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using Newtonsoft.Json;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using Sistema_Subastas.Atributo;

namespace Sistema_Subastas.Controllers
{
    [SesionActiva]

    public class ReporteGraficoController : Controller
    {
        private readonly subastaDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReporteGraficoController(subastaDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GenerarReporte(DateTime fechaInicio, DateTime fechaFin)
        {
            var usuarios = _context.usuarios
                .Where(u => u.fecha_registro >= fechaInicio && u.fecha_registro <= fechaFin)
                .ToList();

            ViewData["fechaInicio"] = fechaInicio.ToString("yyyy-MM-dd");
            ViewData["fechaFin"] = fechaFin.ToString("yyyy-MM-dd");

            return View("Index", usuarios);
        }

        [HttpPost]
        public IActionResult GenerarPDF(DateTime fechaInicio, DateTime fechaFin)
        {
            var usuarios = _context.usuarios
                .Where(u => u.fecha_registro >= fechaInicio && u.fecha_registro <= fechaFin)
                .ToList();

            if (!usuarios.Any())
            {
                TempData["Error"] = "No hay usuarios registrados en el período seleccionado.";
                return RedirectToAction("Index", "ReporteGrafico");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

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

                // Título del reporte
                document.Add(new Paragraph($"Usuarios Registrados del {fechaInicio:dd/MM/yyyy} al {fechaFin:dd/MM/yyyy}")
                    .SetFont(boldFont).SetFontSize(16).SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph("\n"));

                // Tabla de usuarios
                Table table = new Table(3).UseAllAvailableWidth();
                table.AddHeaderCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .Add(new Paragraph("ID").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .Add(new Paragraph("Nombre").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .Add(new Paragraph("Fecha de Registro").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));

                foreach (var usuario in usuarios)
                {
                    table.AddCell(new Cell().Add(new Paragraph(usuario.id.ToString()).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(usuario.nombre).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(usuario.fecha_registro.ToString("dd/MM/yyyy")).SetTextAlignment(TextAlignment.CENTER)));
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

                string timestamp = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/El_Salvador").ToString("yyyyMMdd_HHmmss");
                return File(ms.ToArray(), "application/pdf", $"Reporte_Usuarios_{timestamp}.pdf");
            }
        }



        public IActionResult ReportesUsuarios()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GenerarReporteUsuarios()
        {
            var usuarios = _context.usuarios.ToList();

            return View("ReportesUsuarios", usuarios);
        }

        [HttpPost]
        public IActionResult GenerarPDFUsuarios()
        {
            var usuarios = _context.usuarios.ToList();

            if (!usuarios.Any())
            {
                TempData["Error"] = "No hay usuarios registrados en el período seleccionado.";
                return RedirectToAction("Index", "ReporteGrafico");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

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

                // Título del reporte
                document.Add(new Paragraph($"Usuarios Totales Registrados")
                    .SetFont(boldFont).SetFontSize(16).SetTextAlignment(TextAlignment.CENTER));

                document.Add(new Paragraph("\n"));

                // Tabla de usuarios
                Table table = new Table(3).UseAllAvailableWidth();
                table.AddHeaderCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .Add(new Paragraph("ID").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .Add(new Paragraph("Nombre").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .Add(new Paragraph("Fecha de Registro").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));

                foreach (var usuario in usuarios)
                {
                    table.AddCell(new Cell().Add(new Paragraph(usuario.id.ToString()).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(usuario.nombre).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(usuario.fecha_registro.ToString("dd/MM/yyyy")).SetTextAlignment(TextAlignment.CENTER)));
                }

                // Agregar fila con total de usuarios
                Cell totalCell = new Cell(1, 3)
                    .Add(new Paragraph($"Total de Usuarios Registrados: {usuarios.Count}")
                    .SetFont(boldFont)
                    .SetTextAlignment(TextAlignment.RIGHT));
                table.AddCell(totalCell);

                document.Add(table);

                // Pie de página
                document.Add(new Paragraph("\n"));
                Table footerTable = new Table(1).UseAllAvailableWidth();
                footerTable.AddCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Buy-Things@email.com | (+503) 2490 8943 | El Salvador")));
                document.Add(footerTable);

                document.Close();

                string timestamp = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/El_Salvador").ToString("yyyyMMdd_HHmmss");
                return File(ms.ToArray(), "application/pdf", $"Reporte_Usuarios_{timestamp}.pdf");
            }
        }
        public IActionResult Graficos()
        {
            var subastas = _context.articulos.ToList();

            int abiertas = subastas.Count(s => s.estado_subasta == "Publicado");
            int cerradas = subastas.Count(s => s.estado_subasta == "Vendido" || s.estado_subasta == "Cancelado");

            var datosGrafica = new
            {
                series = new[]
                {
                new
                {
                    name = "Estados de Subastas",
                    colorByPoint = true,
                    data = new[]
                    {
                        new { name = "Abiertas", y = abiertas },
                        new { name = "Cerradas", y = cerradas }
                    }
                }
            }
            };

            ViewBag.DatosGrafica = JsonConvert.SerializeObject(datosGrafica);

            return View();
        }
        public IActionResult SubastasPorUsuario()
        {
            var subastasActivas = _context.articulos
                .Where(s => s.estado_subasta == "Publicado")
                .GroupBy(s => s.usuario_id)
                .Select(g => new
                {
                    UsuarioId = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();

            var usuarios = _context.usuarios
                .Where(u => subastasActivas.Select(s => s.UsuarioId).Contains(u.id))
                .ToDictionary(u => u.id, u => u.nombre);

            var datosGrafica = new
            {
                series = new[]
                {
                new
                {
                    name = "Subastas Activas",
                    colorByPoint = true,
                    data = subastasActivas.Select(s => new
                    {
                        name = usuarios.ContainsKey(s.UsuarioId) ? usuarios[s.UsuarioId] : $"Usuario {s.UsuarioId}",
                        y = s.Cantidad
                    }).ToArray()
                }
            }
            };

            ViewBag.DatosGrafica = JsonConvert.SerializeObject(datosGrafica);

            return View();
        }
    }
}


