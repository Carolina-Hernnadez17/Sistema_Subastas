using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
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
            if (id == null) return NotFound();

            var articulo = await _context.articulos
                .FirstOrDefaultAsync(m => m.Id == id);

            if (articulo == null) return NotFound();

            return View(articulo);
        }

        // GET: Articulos/Create
        public IActionResult Create()
        {
            ViewBag.Categorias = _context.categorias.ToList();
            return View();
        }

        

        [HttpGet]

        public IActionResult Historial()
        {
            int? usuarioId = HttpContext.Session.GetInt32("id_usuario");
            if (usuarioId == null)
                return RedirectToAction("Login", "Usuarios");

            var publicaciones = (from a in _context.articulos
                                 where a.usuario_id == usuarioId
                                 join ac in _context.articulo_categoria on a.Id equals ac.articulo_id into acGroup
                                 from ac in acGroup.DefaultIfEmpty()
                                 join c in _context.categorias on ac.categoria_id equals c.Id into cGroup
                                 from c in cGroup.DefaultIfEmpty()
                                 join ia in _context.imagenes_articulos on a.Id equals ia.articulo_id into iaGroup
                                 from ia in iaGroup.DefaultIfEmpty()
                                 group new { a, c, ia } by a.Id into grouped
                                 select new
                                 {
                                     Articulo = grouped.FirstOrDefault().a,
                                     Categoria = grouped.FirstOrDefault().c != null ? grouped.FirstOrDefault().c.nombre : "Sin categoría",
                                     ImagenUrl = grouped.FirstOrDefault().ia != null ? grouped.FirstOrDefault().ia.url_imagen : ""
                                 }).ToList();

            var categorias = _context.categorias.Select(c => c.nombre).ToList();

            ViewData["Publicaciones"] = publicaciones;
            ViewData["Categorias"] = categorias;

            return View("Historial");
        }

        [HttpPost]
        public IActionResult EliminarArticulo(int id)
        {
            var articulo = _context.articulos.FirstOrDefault(a => a.Id == id);

            if (articulo == null)
            {
                TempData["Error"] = "El artículo no fue encontrado.";
                return RedirectToAction("Index", "Imagenes_articulos");
            }

            var pujas = _context.pujas.Where(p => p.articulo_id == id).ToList();
            if (pujas.Count > 0)
            {
                TempData["Error"] = "El artículo no puede eliminarse porque ya tiene pujas registradas.";
                return RedirectToAction("Details", new { id = id });
            }

            _context.articulos.Remove(articulo);
            _context.SaveChanges();

            TempData["Success"] = "Artículo eliminado correctamente.";
            return RedirectToAction("Historial","Articulos");
        }




        // POST: Articulos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,titulo,descripcion,estado,precio_salida,precio_venta,fecha_inicio,fecha_fin,usuario_id,estado_subasta,visualizacion_puja,fecha_registro")] articulos articulos, int categoria_id, int userId)
        {
            articulos.usuario_id = userId;
            articulos.fecha_registro = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/El_Salvador"); ;
            articulos.estado_subasta = "Publicado";
            articulos.Id = 0;

            if (ModelState.IsValid)
            {
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

            ViewBag.Categorias = _context.categorias.ToList(); // ← importante si fallan validaciones
            return View(articulos);
        }

        // GET: Articulos/Edit/5
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
        public async Task<IActionResult> Edit(articulos articulo, List<imagenes_articulos> Imagenes,List<IFormFile> NuevasImagenes, List<IFormFile> ImagenesAdicionales, int categoria_id)
      
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

                var imagenesActuales = await _context.imagenes_articulos
                                       .Where(i => i.articulo_id == articulo.Id)
                                       .CountAsync();

                if (ImagenesAdicionales != null && ImagenesAdicionales.Count > 0)
                {
                    foreach (var file in ImagenesAdicionales)
                    {
                        // Verificar si ya alcanzamos el límite de 5 imágenes
                        if (imagenesActuales >= 5)
                            break;

                        if (file != null && file.Length > 0)
                        {
                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(file.FileName, file.OpenReadStream()),
                                Folder = "Subasta"
                            };

                            var uploadResult = await cloudinary.UploadAsync(uploadParams);

                            if (uploadResult != null && !string.IsNullOrEmpty(uploadResult.SecureUrl?.ToString()))
                            {
                                var nuevaImagen = new imagenes_articulos
                                {
                                    articulo_id = articulo.Id,
                                    url_imagen = uploadResult.SecureUrl.ToString()
                                };
                                _context.Add(nuevaImagen);
                                imagenesActuales++;
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
                TempData["MensajeE"] = "Artículo e imágenes actualizados correctamente.";
                return RedirectToAction("Index", "Imagenes_articulos");
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarImagen(int id)
        {
            try
            {
                var imagen = await _context.imagenes_articulos.FindAsync(id);
                if (imagen == null)
                {
                    return NotFound();
                }


                int articuloId = imagen.articulo_id;


                _context.imagenes_articulos.Remove(imagen);
                await _context.SaveChangesAsync();




                TempData["MensajeE"] = "Imagen eliminada correctamente.";
                return RedirectToAction("Edit", new { id = articuloId });
            }
            catch (Exception ex)
            {
                TempData["MensajeE"] = "Error al eliminar la imagen: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        


        // GET: Articulos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var articulo = await _context.articulos
                .FirstOrDefaultAsync(m => m.Id == id);

            if (articulo == null) return NotFound();

            return View(articulo);
        }

        // POST: Articulos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var articulo = await _context.articulos.FindAsync(id);
            if (articulo != null)
            {
                _context.articulos.Remove(articulo);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool articulosExists(int id)
        {
            return _context.articulos.Any(e => e.Id == id);
        }
    }
}
