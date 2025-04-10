﻿using System;
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
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

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
            var imagenes = await _context.imagenes_articulos
                                .GroupBy(img => img.articulo_id)
                                .Select(g => g.First())
                                .ToListAsync();

            var articulos = await _context.articulos.ToListAsync();
            var categorias = await _context.categorias.ToListAsync();
            var articuloCategorias = await _context.articulo_categoria.ToListAsync();

            var articulo = _context.articulos.FirstOrDefault();

            ViewBag.Articulos = articulos;
            ViewBag.ArticuloCategorias = articuloCategorias;
            ViewBag.Categorias = categorias;

            
            return View(imagenes);


        }
        // GET: Imagenes_articulos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }



            var imagenes_articulos = _context.imagenes_articulos.Where(m => m.articulo_id == id).ToList();


            
            var articuloCategoria = _context.articulo_categoria.FirstOrDefault(ac => ac.articulo_id == id);
            var categoria = _context.categorias.FirstOrDefault(c => c.Id == articuloCategoria.categoria_id);
            ViewBag.CategoriaNombre = categoria.nombre;


           
            var datos_vendedor = _context.articulos.FirstOrDefault(a => a.Id == id);
            var vendedor = _context.usuarios.FirstOrDefault(u => u.id == datos_vendedor.usuario_id);

            ViewBag.Vendedor = vendedor.nombre;
            ViewBag.IdVendedor = vendedor.id;
            //Muestra los articulos
            var articulo = _context.articulos.FirstOrDefault(a => a.Id == id);
            ViewBag.Articulos = articulo;

           

            DateTime fecha_cierre = Convert.ToDateTime(articulo.fecha_fin);
            DateTime fecha_actual = DateTime.Now;

            TimeSpan fecha_res = fecha_cierre - fecha_actual  ;

            if (fecha_res < TimeSpan.Zero)
            {
                fecha_res = TimeSpan.Zero;
            }

            if (fecha_res.Days >= articulo.fecha_fin.Day || fecha_res == TimeSpan.Zero)
            {
                
                ViewBag.Fecha = "Subasta Cerrada";
                ViewBag.Estado = articulo.estado_subasta;
                
            }
            else
            {

                ViewBag.VerPuja = articulo.visualizacion_puja;
                ViewBag.Estado = articulo.estado_subasta;
                ViewBag.Fecha = $"{fecha_res.Days} días {fecha_res.Hours} horas {fecha_res.Minutes} minutos {fecha_res.Seconds} segundos";
            }

            var cantidadP = _context.pujas.Count(p => p.articulo_id == id);

            if (cantidadP == 0 || cantidadP == null)
            {
                ViewBag.CantidadP = 0;
            }
            else if (cantidadP > 0)
            {
                ViewBag.CantidadP = cantidadP;
            }


            if (imagenes_articulos == null)
            {
                return NotFound();
            }
            
            return View(imagenes_articulos);
        }
      

        // GET: Imagenes_articulos/Create
        public IActionResult Create(int articulo_id)
        {
            var model = new imagenes_articulos { articulo_id = articulo_id };
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int articulo_id, List<IFormFile> files)
        {
            

            try
            {
                
                var account = new Account(
                    "daxbwcgw2",
                    "346927586337937",
                    "YqqRBDv2Ha3x_qxjNknM8_sT83Q"
                );
                var cloudinary = new Cloudinary(account);

                foreach (var file in files)
                {
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
                           
                            var imagen = new imagenes_articulos
                            {
                                articulo_id = articulo_id,
                                url_imagen = uploadResult.SecureUrl.ToString()
                            };

                            _context.imagenes_articulos.Add(imagen);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Imágenes agregadas exitosamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al subir imágenes: " + ex.Message);
                return View();
            }
        }



        [HttpPost("Eliminar/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var imagen = _context.imagenes_articulos.FirstOrDefault(i => i.id == id);
            if (imagen == null)
            {
                return NotFound();
            }

            _context.imagenes_articulos.Remove(imagen);
            _context.SaveChanges();

            return Ok(); // Código 200
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
