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
using MySql.Data.MySqlClient;

namespace Sistema_Subastas.Controllers
{
    public class Imagenes_articulosController : Controller
    {
        private readonly subastaDbContext _context;
        private MySqlConnection conexion;
        public Imagenes_articulosController(subastaDbContext context)
        {
            _context = context;
        }

        // GET: Imagenes_articulos
        public async Task<IActionResult> Index()
        {
            return View(await _context.imagenes_articulos.ToListAsync());
        }

        // GET: Imagenes_articulos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var imagenes_articulos = await _context.imagenes_articulos
                .FirstOrDefaultAsync(m => m.id == id);
            if (imagenes_articulos == null)
            {
                return NotFound();
            }

            return View(imagenes_articulos);
        }

        // GET: Imagenes_articulos/Create
        public IActionResult Create()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,articulo_id,url_imagen")] imagenes_articulos img_articulos, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    ModelState.AddModelError("file", "Debe seleccionar una imagen válida.");
                    return View(img_articulos);
                }

                // Configuración de Cloudinary
                var account = new Account(
                    "daxbwcgw2",
                    "346927586337937",
                    "YqqRBDv2Ha3x_qxjNknM8_sT83Q"
                );
                var cloudinary = new Cloudinary(account);

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = "Subasta"
                };

                var uploadResult = await Task.Run(() => cloudinary.Upload(uploadParams));

                if (uploadResult == null || string.IsNullOrEmpty(uploadResult.SecureUrl?.ToString()))
                {
                    ModelState.AddModelError("file", "Error al subir la imagen a Cloudinary.");
                    return View(img_articulos);
                }

                // Guardar URL en el modelo
                img_articulos.url_imagen = uploadResult.SecureUrl.ToString();

                // Guardar en la base de datos usando Entity Framework
                _context.Add(img_articulos);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Imagen agregada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al agregar la imagen: " + ex.Message);
                return View(img_articulos);
            }
        }





        //[HttpPost]
        //public async Task<ActionResult> Create(imagenes_articulos img_articulos, IFormFile file)
        //{
        //    try
        //    {
        //        string rutaImagen = null;

        //        if (file != null && file.Length > 0)
        //        {
        //            var account = new Account(
        //                "daxbwcgw2",
        //                "346927586337937",
        //                "YqqRBDv2Ha3x_qxjNknM8_sT83Q"
        //            );

        //            var cloudinary = new Cloudinary(account);
        //            var uploadParams = new ImageUploadParams()
        //            {
        //                File = new FileDescription(file.FileName, file.OpenReadStream()),
        //                Folder = "Subasta"
        //            };

        //            var uploadResult = await Task.Run(() => cloudinary.Upload(uploadParams));
        //            rutaImagen = uploadResult.SecureUrl.ToString();
        //        }

        //        img_articulos.url_imagen = rutaImagen;

        //        using (var conn = conexion)
        //        {
        //            string query = "INSERT INTO imagenes_articulos (articulo_id,url_imagen)" +
        //                           " VALUES (@articulo_id,@url_imagen)";
        //            MySqlCommand cmd = new MySqlCommand(query, conn);
        //            cmd.Parameters.AddWithValue("@articulo_id", img_articulos.articulo_id);
        //            cmd.Parameters.AddWithValue("@url_imagen", img_articulos.url_imagen);


        //            int result = cmd.ExecuteNonQuery();

        //            if (result > 0)
        //            {
        //                TempData["SuccessMessage"] = "Obra agregada exitosamente";
        //                return RedirectToAction("Index");
        //            }
        //            else
        //            {
        //                ViewBag.ErrorMessage = "No se pudo agregar .";
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Error = "Error al agregar obra: " + ex.Message;
        //    }

        //    return View(img_articulos);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("id,articulo_id,url_imagen")] imagenes_articulos imagenes_articulos, IFormFile file)
        //{

        //    
        //    if (ModelState.IsValid)
        //    {

        //        _context.Add(imagenes_articulos);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(imagenes_articulos);
        //}

        // GET: Imagenes_articulos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var imagenes_articulos = await _context.imagenes_articulos.FindAsync(id);
            if (imagenes_articulos == null)
            {
                return NotFound();
            }
            return View(imagenes_articulos);
        }

        // POST: Imagenes_articulos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,articulo_id,url_imagen")] imagenes_articulos imagenes_articulos)
        {
            if (id != imagenes_articulos.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(imagenes_articulos);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!imagenes_articulosExists(imagenes_articulos.id))
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
            return View(imagenes_articulos);
        }

        // GET: Imagenes_articulos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var imagenes_articulos = await _context.imagenes_articulos
                .FirstOrDefaultAsync(m => m.id == id);
            if (imagenes_articulos == null)
            {
                return NotFound();
            }

            return View(imagenes_articulos);
        }

        // POST: Imagenes_articulos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var imagenes_articulos = await _context.imagenes_articulos.FindAsync(id);
            if (imagenes_articulos != null)
            {
                _context.imagenes_articulos.Remove(imagenes_articulos);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool imagenes_articulosExists(int id)
        {
            return _context.imagenes_articulos.Any(e => e.id == id);
        }
    }
}
