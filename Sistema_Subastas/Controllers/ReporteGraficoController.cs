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

namespace Sistema_Subastas.Controllers
{
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

                // Agregar el logo
                string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    Image img = new Image(ImageDataFactory.Create(logoPath)).ScaleToFit(80, 80);
                    document.Add(img.SetHorizontalAlignment(HorizontalAlignment.CENTER));
                }

                // Encabezado del reporte
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                document.Add(new Paragraph("Buy-Things").SetFont(boldFont).SetFontSize(20).SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph($"Fecha de generación: {DateTime.Now:dd/MM/yyyy}").SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph("Dirección: Santa Ana").SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph("Teléfono: 7878-8989").SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph("\n"));

                // Título del reporte con fechas
                document.Add(new Paragraph($"Usuarios Registrados del {fechaInicio:dd/MM/yyyy} al {fechaFin:dd/MM/yyyy}")
                    .SetFont(boldFont).SetFontSize(16).SetTextAlignment(TextAlignment.CENTER));

                document.Add(new Paragraph("\n"));

                // Tabla de usuarios
                Table table = new Table(3).UseAllAvailableWidth();
                table.AddHeaderCell(new Cell().Add(new Paragraph("ID").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha de Registro").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));

                foreach (var usuario in usuarios)
                {
                    table.AddCell(new Cell().Add(new Paragraph(usuario.id.ToString()).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(usuario.nombre).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(usuario.fecha_registro.ToString("dd/MM/yyyy")).SetTextAlignment(TextAlignment.CENTER)));
                }

                document.Add(table);
                document.Close();

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
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

                // Agregar el logo
                string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    Image img = new Image(ImageDataFactory.Create(logoPath)).ScaleToFit(80, 80);
                    document.Add(img.SetHorizontalAlignment(HorizontalAlignment.CENTER));
                }

                // Encabezado del reporte
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                document.Add(new Paragraph("Buy-Things").SetFont(boldFont).SetFontSize(20).SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph($"Fecha de generación: {DateTime.Now:dd/MM/yyyy}").SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph("Dirección: Santa Ana").SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph("Teléfono: 7878-8989").SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph("\n"));

                // Título del reporte con fechas
                document.Add(new Paragraph($"Usuarios Totales Registrados")
                    .SetFont(boldFont).SetFontSize(16).SetTextAlignment(TextAlignment.CENTER));

                document.Add(new Paragraph("\n"));

                // Tabla de usuarios
                Table table = new Table(3).UseAllAvailableWidth();
                table.AddHeaderCell(new Cell().Add(new Paragraph("ID").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha de Registro").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));

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
                document.Close();

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
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


