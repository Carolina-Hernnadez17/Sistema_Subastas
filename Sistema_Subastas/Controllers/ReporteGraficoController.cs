//using iText.IO.Image;
//using Microsoft.AspNetCore.Mvc;
//using iText.Kernel.Pdf;
//using iText.Layout;
//using iText.Layout.Element;
//using Sistema_Subastas.Models;
//using iText.IO.Font.Constants;
//using iText.Kernel.Font;

//namespace Sistema_Subastas.Controllers
//{
//    public class ReporteGraficoController : Controller
//    {
//        private readonly subastaDbContext _context;
//        private readonly IWebHostEnvironment _webHostEnvironment;

//        public ReporteGraficoController(subastaDbContext context, IWebHostEnvironment webHostEnvironment)
//        {
//            _context = context;
//            _webHostEnvironment = webHostEnvironment;
//        }

//        // Vista para seleccionar fechas
//        public IActionResult Index()
//        {
//            return View();
//        }

//        // Método para generar y descargar el PDF
//        [HttpPost]
//        public IActionResult GenerarPDF(DateTime fechaInicio, DateTime fechaFin)
//        {
//            var usuarios = _context.usuarios
//                .Where(u => u.fecha_registro >= fechaInicio && u.fecha_registro <= fechaFin)
//                .ToList();

//            using (MemoryStream ms = new MemoryStream())
//            {
//                PdfWriter writer = new PdfWriter(ms);
//                PdfDocument pdf = new PdfDocument(writer);
//                Document document = new Document(pdf);

//                // Agregar el logo
//                string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "logo.png");
//                if (System.IO.File.Exists(logoPath))
//                {
//                    Image img = new Image(ImageDataFactory.Create(logoPath)).ScaleToFit(100, 100);
//                    document.Add(img);
//                }

//                // Agregar encabezado
//                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

//                document.Add(new Paragraph("Buy-Things").SetFont(boldFont).SetFontSize(18));
//                document.Add(new Paragraph($"Fecha de generación: {DateTime.Now:dd/MM/yyyy}"));
//                document.Add(new Paragraph("Dirección: Santa Ana"));
//                document.Add(new Paragraph("Teléfono: 7878-8989"));
//                document.Add(new Paragraph("\n"));

//                // Título del reporte
//                document.Add(new Paragraph($"Usuarios Registrados ({fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy})")
//                    .SetFont(boldFont).SetFontSize(16));

//                // Tabla de usuarios
//                Table table = new Table(3);
//                table.AddHeaderCell("ID");
//                table.AddHeaderCell("Nombre");
//                table.AddHeaderCell("Fecha de Registro");

//                foreach (var usuario in usuarios)
//                {
//                    table.AddCell(usuario.id.ToString());
//                    table.AddCell(usuario.nombre);
//                    table.AddCell(usuario.fecha_registro.ToString("dd/MM/yyyy"));
//                }

//                document.Add(table);
//                document.Close();

//                return File(ms.ToArray(), "application/pdf", "Reporte_Usuarios.pdf");
//            }
//        }
//    }
//}

using iText.IO.Image;
using Microsoft.AspNetCore.Mvc;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Sistema_Subastas.Models;
using iText.IO.Font.Constants;
using iText.Kernel.Font;

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
    }
}


