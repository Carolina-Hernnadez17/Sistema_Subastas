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

            // Obtener la categoría actual del artículo
            var articuloCategoria = _context.articulo_categoria
                .FirstOrDefault(ac => ac.articulo_id == id);

            int? categoriaActual = articuloCategoria?.categoria_id;

            ViewBag.CategoriasA = _context.categorias.ToList();
            ViewBag.CategoriaActual = categoriaActual;
            ViewBag.Articulo = articulo;
            ViewBag.Imagenes = imagenes;

            return View(articulo);
        }

        // POST: Articulos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(articulos articulo, List<imagenes_articulos> Imagenes, List<IFormFile> NuevasImagenes, int categoria_id)
        {
            try
            {
                               
                _context.Update(articulo);

                // Actualizar categoría
                var articuloCategoria = _context.articulo_categoria.FirstOrDefault(c => c.articulo_id == articulo.Id);
                if (articuloCategoria != null)
                {
                    articuloCategoria.categoria_id = categoria_id;
                    _context.Update(articuloCategoria);
                }
                

                // Configuración de Cloudinary
                var account = new Account("daxbwcgw2", "346927586337937", "YqqRBDv2Ha3x_qxjNknM8_sT83Q");
                var cloudinary = new Cloudinary(account);

                // Actualizar imágenes existentes
                if (NuevasImagenes != null)
                {
                    foreach (var imagen in Imagenes)
                    {
                        // Encontrar el índice de esta imagen
                        int index = Imagenes.FindIndex(i => i.id == imagen.id);

                        // Buscar si hay un archivo nuevo para este índice
                        var fileKey = $"NuevasImagenes[{index}]";
                        var file = Request.Form.Files.FirstOrDefault(f => f.Name == fileKey);

                        if (file != null && file.Length > 0)
                        {
                            var imagenDB = await _context.imagenes_articulos.FindAsync(imagen.id);
                            if (imagenDB != null)
                            {
                                var uploadParams = new ImageUploadParams()
                                {
                                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                                    Folder = "Subasta"
                                };

                                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                                if (uploadResult != null && !string.IsNullOrEmpty(uploadResult.SecureUrl?.ToString()))
                                {
                                    imagenDB.url_imagen = uploadResult.SecureUrl.ToString();
                                    _context.Update(imagenDB);
                                }
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["MensajeE"] = "Artículo e imágenes actualizados correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar: " + ex.Message);

                
                ViewBag.CategoriasA = _context.categorias.ToList();
                ViewBag.CategoriaActual = categoria_id;
                ViewBag.Articulo = articulo;
                ViewBag.Imagenes = _context.imagenes_articulos.Where(i => i.articulo_id == articulo.Id).ToList();

                return View(articulo);
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
