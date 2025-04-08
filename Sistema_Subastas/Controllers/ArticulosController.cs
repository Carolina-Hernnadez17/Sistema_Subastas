using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema_Subastas.Models;

namespace Sistema_Subastas.Controllers
{
    public class ArticulosController : Controller
    {
        private readonly subastaDbContext _context;

        public ArticulosController(subastaDbContext context)
        {
            _context = context;
        }

        // GET: Articulos
        public async Task<IActionResult> Index()
        {
            return View(await _context.articulos.ToListAsync());
        }

        // GET: Articulos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articulos = await _context.articulos
                .FirstOrDefaultAsync(m => m.Id == id);

           

            if (articulos == null)
            {
                return NotFound();
            }

            return View(articulos);
        }

        // GET: Articulos/Create
        public IActionResult Create()
        {
            ViewBag.Categorias = _context.categorias.ToList();
            return View();
        }

        // POST: Articulos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,titulo,descripcion,estado,precio_salida,precio_venta,fecha_inicio,fecha_fin,usuario_id,estado_subasta, fecha_registro")] articulos articulos, int categoria_id, int userId)
        {
            articulos.usuario_id = userId;
            articulos.Id = 0;

            if (ModelState.IsValid)
            {

                // Guardar el artículo
                _context.Add(articulos);
                await _context.SaveChangesAsync();

                
                int articuloId = articulos.Id;

                if (categoria_id > 0) 
                {
                    var articuloCategoria = new articulo_categoria
                    {
                        articulo_id = articuloId,
                        categoria_id = categoria_id,
                        
                    };

                    _context.articulo_categoria.Add(articuloCategoria);
                    await _context.SaveChangesAsync();
                    
                }
                TempData["MensajeE"] = "El articulo se guardo correctamente";
                return RedirectToAction("Create", "Imagenes_articulos", new { articulo_id = articuloId });
            }



            return View(articulos);
        }

        // GET: Articulos/Edit/5
        public IActionResult Edit(int id)
        {
            var articulo = _context.articulos.FirstOrDefault(a => a.Id == id);
            if (articulo == null)
            {
                return NotFound();
            }

            var imagenes = _context.imagenes_articulos
                .Where(i => i.articulo_id == id)
                .ToList();

            ViewBag.Articulo = articulo;
            ViewBag.Imagenes = imagenes;

            return View(articulo); 
        }


        // POST: Articulos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(articulos articulo, List<imagenes_articulos> Imagenes, List<IFormFile> NuevasImagenes)
        {
            try
            {
                _context.Update(articulo);

                var account = new Account("daxbwcgw2", "346927586337937", "YqqRBDv2Ha3x_qxjNknM8_sT83Q");
                var cloudinary = new Cloudinary(account);

                for (int i = 0; i < Imagenes.Count; i++)
                {
                    var imagenDB = await _context.imagenes_articulos.FindAsync(Imagenes[i].id);

                    if (imagenDB != null && i < NuevasImagenes.Count && NuevasImagenes[i] != null)
                    {
                        var file = NuevasImagenes[i];
                        if (file.Length > 0)
                        {
                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(file.FileName, file.OpenReadStream()),
                                Folder = "Subasta"
                            };

                            var uploadResult = await Task.Run(() => cloudinary.Upload(uploadParams));

                            if (uploadResult != null && !string.IsNullOrEmpty(uploadResult.SecureUrl?.ToString()))
                            {
                                imagenDB.url_imagen = uploadResult.SecureUrl.ToString();
                                _context.Update(imagenDB);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Artículo e imágenes actualizados correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
                return View();
            }
        }

        // GET: Articulos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articulos = await _context.articulos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (articulos == null)
            {
                return NotFound();
            }

            return View(articulos);
        }

        // POST: Articulos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var articulos = await _context.articulos.FindAsync(id);
            if (articulos != null)
            {
                _context.articulos.Remove(articulos);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool articulosExists(int id)
        {
            return _context.articulos.Any(e => e.Id == id);
        }

        
    }
}
