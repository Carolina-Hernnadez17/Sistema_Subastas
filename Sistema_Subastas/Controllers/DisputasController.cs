using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema_Subastas.Atributo;
using Sistema_Subastas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Paragraph = iText.Layout.Element.Paragraph;


namespace Sistema_Subastas.Controllers
{
    [SesionActiva]

    public class DisputasController : Controller
    {
        private readonly subastaDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DisputasController(subastaDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Disputas
        public async Task<IActionResult> Index()
        {
            var disputas = await _context.disputas.ToListAsync();

            var articulos = _context.articulos.ToDictionary(a => a.Id, a => a.titulo);
            var usuarios = _context.usuarios.ToDictionary(u => u.id, u => u.nombre);

            ViewBag.Articulos = articulos;
            ViewBag.Usuarios = usuarios;

            return View(disputas);
        }


        // GET: Disputas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var disputas = await _context.disputas
                .FirstOrDefaultAsync(m => m.id == id);
            if (disputas == null)
            {
                return NotFound();
            }

            return View(disputas);
        }

        // GET: Disputas/Create
        public IActionResult Create(int articulo_id, int vendedor_id)
        {
            var compradorId = HttpContext.Session.GetInt32("id_usuario");
            var nombre = HttpContext.Session.GetString("NombreUser");

            if (compradorId == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            var articulo = _context.articulos.Find(articulo_id);
            var vendedor = _context.usuarios.Find(vendedor_id);

            if (articulo == null || vendedor == null)
            {
                TempData["MensajeError"] = "No se pudo cargar la información del artículo o vendedor.";
                return RedirectToAction("Index", "Imagenes_articulos");
            }

            // ❗ Validación: verificar si el usuario ganó la subasta
            var pujaGanadora = _context.pujas
                .Where(p => p.articulo_id == articulo_id)
                .OrderByDescending(p => p.monto)
                .FirstOrDefault();

            if (pujaGanadora == null || pujaGanadora.usuario_id != compradorId)
            {
                TempData["MensajeError"] = "Solo puedes presentar una disputa si ganaste la subasta.";
                return RedirectToAction("Index", "Imagenes_articulos");
            }

            // ❗ Validación: solo puede presentar disputa dentro de los 3 días posteriores al cierre
            if (articulo.fecha_fin.AddDays(3) < TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/El_Salvador"))
            {
                TempData["MensajeError"] = "Ya no puedes presentar una disputa porque han pasado más de 3 días desde el cierre.";
                return RedirectToAction("Index", "Imagenes_articulos");
            }

            // ViewBag para la vista
            ViewBag.ArticuloId = articulo_id;
            ViewBag.ArticuloTitulo = articulo?.titulo ?? "(Sin título)";
            ViewBag.CompradorId = compradorId ?? 0;
            ViewBag.CompradorNombre = !string.IsNullOrEmpty(nombre) ? nombre : "Usuario actual";
            ViewBag.VendedorId = vendedor_id;
            ViewBag.VendedorNombre = vendedor?.nombre ?? "Vendedor";

            return View();
        }




        // POST: Disputas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,articulo_id,comprador_id,vendedor_id,motivo,descripcion,estado,fecha")] disputas disputas)
        {
            if (ModelState.IsValid)
            {
                _context.Add(disputas);
                await _context.SaveChangesAsync();

                TempData["MensajeDisputa"] = "La disputa fue registrada correctamente.";
                return RedirectToAction("Index", "Imagenes_articulos");
            }

            var articulo = await _context.articulos.FindAsync(disputas.articulo_id);
            var vendedor = await _context.usuarios.FindAsync(disputas.vendedor_id);
            var nombre = HttpContext.Session.GetString("NombreUser");
            var compradorId = HttpContext.Session.GetInt32("id_usuario");

            ViewBag.ArticuloId = disputas.articulo_id;
            ViewBag.ArticuloTitulo = articulo?.titulo ?? "(Sin título)";
            ViewBag.CompradorId = compradorId ?? 0;
            ViewBag.CompradorNombre = nombre ?? "Usuario actual";
            ViewBag.VendedorId = disputas.vendedor_id;
            ViewBag.VendedorNombre = vendedor?.nombre ?? "Vendedor";

            foreach (var modelState in ModelState)
            {
                foreach (var error in modelState.Value.Errors)
                {
                    Console.WriteLine($"Error en {modelState.Key}: {error.ErrorMessage}");
                }
            }


            return View(disputas);
        }


        // GET: Disputas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var disputas = await _context.disputas.FindAsync(id);
            if (disputas == null)
            {
                return NotFound();
            }
            return View(disputas);
        }

        // POST: Disputas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,articulo_id,comprador_id,vendedor_id,motivo,descripcion,estado,fecha")] disputas disputas)
        {
            if (id != disputas.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(disputas);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!disputasExists(disputas.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(disputas);
        }

        // GET: Disputas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var disputas = await _context.disputas
                .FirstOrDefaultAsync(m => m.id == id);
            if (disputas == null)
            {
                return NotFound();
            }

            return View(disputas);
        }

        // POST: Disputas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var disputas = await _context.disputas.FindAsync(id);
            if (disputas != null)
            {
                _context.disputas.Remove(disputas);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool disputasExists(int id)
        {
            return _context.disputas.Any(e => e.id == id);
        }

        public async Task<IActionResult> DescargarReporteDisputas()
        {
            var disputas = await _context.disputas.ToListAsync();

            using (var ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Estilos
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

                // Encabezado principal
                var titleTable = new Table(1).UseAllAvailableWidth();
                titleTable.AddCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Buy-Things").SetFont(boldFont).SetFontSize(20)));
                document.Add(titleTable);

                // Fecha y dirección
                document.Add(new Paragraph($"Fecha de generación: {TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/El_Salvador"):dd/MM/yyyy HH:mm}")
                    .SetTextAlignment(TextAlignment.CENTER).SetFontSize(12));
                document.Add(new Paragraph("Dirección: Santa Ana | Tu mejor opción en subastas")
                    .SetTextAlignment(TextAlignment.CENTER).SetFontSize(12));
                document.Add(new Paragraph("\n"));

                // Título del reporte
                document.Add(new Paragraph("Disputas Registradas")
                    .SetFont(boldFont).SetFontSize(16).SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph("\n"));

                // Tabla
                var table = new Table(6).UseAllAvailableWidth();
                string[] headers = { "Artículo ID", "Comprador", "Vendedor", "Motivo", "Estado", "Fecha" };
                foreach (var header in headers)
                {
                    table.AddHeaderCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                        .Add(new Paragraph(header).SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                }

                foreach (var d in disputas)
                {
                    table.AddCell(new Cell().Add(new Paragraph(d.articulo_id.ToString()).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(d.comprador_id.ToString()).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(d.vendedor_id.ToString()).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(d.motivo ?? "N/A").SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(d.estado ?? "N/A").SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(d.fecha.ToString("dd/MM/yyyy HH:mm")).SetTextAlignment(TextAlignment.CENTER)));
                }

                document.Add(table);
                document.Add(new Paragraph("\n"));

                // Footer
                var footer = new Table(1).UseAllAvailableWidth();
                footer.AddCell(new Cell().SetBackgroundColor(headerColor).SetFontColor(textColor)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Buy-Things@email.com | (+503) 2490 8943 | El Salvador")));
                document.Add(footer);

                document.Close();

                return File(ms.ToArray(), "application/pdf", "Reporte_Disputas.pdf");
            }
        }



    }



}
