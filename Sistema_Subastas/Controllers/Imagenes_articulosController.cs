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
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using Sistema_Subastas.Services;

namespace Sistema_Subastas.Controllers
{
    public class Imagenes_articulosController : Controller
    {
        private readonly subastaDbContext _context;
        private MySqlConnection conexion;
        private readonly EmailService _emailService;

        public Imagenes_articulosController(subastaDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;

        }

        // GET: Imagenes_articulos
        public async Task<IActionResult> Index()
        {

            var ahora = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/El_Salvador");

            // Verificamos si hay alguna subasta que necesita ser cerrada
            bool hayPorFinalizar = _context.articulos
                .Any(a => a.estado_subasta == "Finalizada");

            if (hayPorFinalizar)
            {
                //MarcarSubastasFinalizadas();
                DeterminarGanadores();
                await CerrarSubastasFinalizadas();
            }

            //var imagenes = await _context.imagenes_articulos
            //                    .GroupBy(img => img.articulo_id)
            //                    .Select(g => g.First())
            //                    .ToListAsync();

            var articulos = await _context.articulos.ToListAsync();
            var categorias = await _context.categorias.ToListAsync();
            var articuloCategorias = await _context.articulo_categoria.ToListAsync();


            var imagenes = await _context.imagenes_articulos.ToListAsync();



            var articulo = _context.articulos.FirstOrDefault();


            ViewBag.Articulos = articulos;
            ViewBag.ArticuloCategorias = articuloCategorias;
            ViewBag.Categorias = categorias;

            var pujaMayor = _context.pujas
                .Where(p => p.articulo_id == articulo.Id)
                .OrderByDescending(p => p.monto)
                .ThenBy(p => p.fecha_puja)
                .FirstOrDefault();
            if (pujaMayor != null)
            {
                articulo.precio_venta = pujaMayor.monto;
                _context.SaveChanges();
            }

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

            var articulo = _context.articulos.FirstOrDefault(a => a.Id == id);
            ViewBag.Articulos = articulo;

            DateTime fecha_cierre = articulo.fecha_fin;
            DateTime fecha_actual = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/El_Salvador"); ;

            TimeSpan tiempoRestante = fecha_cierre - fecha_actual;

            if (tiempoRestante <= TimeSpan.Zero)
            {
                if (articulo.estado_subasta == "Publicado")
                {
                    articulo.estado_subasta = "Finalizada";
                    _context.articulos.Update(articulo);
                    await _context.SaveChangesAsync();
                }

                ViewBag.Fecha = "Subasta Cerrada";
            }
            else
            {
                ViewBag.Fecha = $"{tiempoRestante.Days} días {tiempoRestante.Hours} horas {tiempoRestante.Minutes} minutos {tiempoRestante.Seconds} segundos";
            }

            ViewBag.Estado = articulo.estado_subasta;
            ViewBag.VerPuja = articulo.visualizacion_puja;

            var cantidadP = _context.pujas.Count(p => p.articulo_id == id);
            ViewBag.CantidadP = cantidadP;

            if (imagenes_articulos == null)
            {
                return NotFound();
            }

            var pujaMayor = _context.pujas
                .Where(p => p.articulo_id == articulo.Id)
                .OrderByDescending(p => p.monto)
                .ThenBy(p => p.fecha_puja)
                .FirstOrDefault();

            if (pujaMayor != null)
            {
                articulo.precio_venta = pujaMayor.monto;
                _context.SaveChanges();
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
        // Parte de notificaciones, cierre de subastas y ganadores
        public void MarcarSubastasFinalizadas()
        {
            var ahora = DateTime.UtcNow;
            //var ahoraRedondeado = new DateTime(ahora.Year, ahora.Month, ahora.Day, ahora.Hour, ahora.Minute, ahora.Second);

            var articulosParaFinalizar = _context.articulos
                .Where(a => a.estado_subasta == "Publicado" &&
                            a.fecha_fin == ahora)
                .ToList();

            foreach (var articulo in articulosParaFinalizar)
            {
                articulo.estado_subasta = "Finalizada";
            }

            _context.SaveChanges();
        }

        public void DeterminarGanadores()
        {
            var articulosFinalizados = _context.articulos
                .Where(a => a.estado_subasta == "Finalizada")
                .ToList();

            foreach (var articulo in articulosFinalizados)
            {
                var pujasDelArticulo = _context.pujas
                    .Where(p => p.articulo_id == articulo.Id)
                    .OrderByDescending(p => p.monto)
                    .ThenBy(p => p.fecha_puja)
                    .ToList();

                if (pujasDelArticulo.Any())
                {
                    var pujaGanadora = pujasDelArticulo.First();
                    pujaGanadora.estado_pujas = "Ganador";

                    foreach (var puja in pujasDelArticulo.Skip(1))
                    {
                        puja.estado_pujas = "No ganador";
                    }

                    articulo.estado_subasta = "Vendido";
                }
                else
                {
                    articulo.estado_subasta = "No vendido";
                }
            }

            _context.SaveChanges();
        }

        public async Task<IActionResult> CerrarSubastasFinalizadas()
        {
            // Subastas no vendidas
            var subastasNoVendidas = _context.articulos
                .Where(a => a.estado_subasta == "No vendido")
                .ToList();

            foreach (var subasta in subastasNoVendidas)
            {
               // DateTime fecha_cierre = Convert.ToDateTime(subasta.fecha_fin);
                string mensaje = $"📢 Tu subasta {subasta.titulo} ha terminado sin pujas el {subasta.fecha_fin:dd/MM/yyyy HH:mm}.";

                bool yaExiste = _context.notificaciones.Any(n =>
                    n.usuario_id == subasta.usuario_id && n.mensaje == mensaje);

                if (!yaExiste)
                {
                    _context.notificaciones.Add(new notificaciones
                    {
                        usuario_id = subasta.usuario_id,
                        mensaje = mensaje,
                        leido = false,
                        fecha = DateTime.UtcNow
                    });

                    var usuario = await _context.usuarios.FindAsync(subasta.usuario_id);
                    if (usuario != null)
                    {
                        _emailService.EnviarCorreo(
                            usuario.correo,
                            "Notificación de subasta sin pujas",
                            mensaje
                        );
                    }
                }
            }

            // Subastas vendidas
            var subastasVendidas = _context.articulos
                .Where(a => a.estado_subasta == "Vendido")
                .ToList();

            foreach (var subasta in subastasVendidas)
            {
                var pujas = _context.pujas
                    .Where(p => p.articulo_id == subasta.Id && p.estado_pujas == "Ganador")
                    .ToList();

                foreach (var puj in pujas)
                {
                    //string mensajeCreador = $"📢 Tu subasta {subasta.titulo} ha sido vendida. El ganador es el usuario con código: {puj.usuario_id}, monto final: {puj.monto}, finalizó el {subasta.fecha_fin:dd/MM/yyyy HH:mm}.";
                    //string mensajeGanador = $"📢 Has ganado la subasta {subasta.titulo} (ID: {subasta.Id}), ¡felicidades!";
                    var ganadorUser = await _context.usuarios.FindAsync(puj.usuario_id);


                    string mensajeCreador = $"📢 Tu subasta {subasta.titulo} ha sido vendida. El ganador es el usuario con código: {puj.usuario_id}, nombre: {ganadorUser.nombre}, correo: {ganadorUser.correo} monto final: {puj.monto}, finalizó el {subasta.fecha_fin:dd/MM/yyyy HH:mm}.";
                    string mensajeGanador = $"📢 Has ganado la subasta {subasta.titulo} (ID: {subasta.Id}), descripción: {subasta.descripcion}, ¡felicidades!";

                    // Notificar al creador
                    if (!_context.notificaciones.Any(n => n.usuario_id == subasta.usuario_id && n.mensaje == mensajeCreador))
                    {
                        _context.notificaciones.Add(new notificaciones
                        {
                            usuario_id = subasta.usuario_id,
                            mensaje = mensajeCreador,
                            leido = false,
                            fecha = DateTime.UtcNow
                        });

                        var creador = await _context.usuarios.FindAsync(subasta.usuario_id);
                        if (creador != null)
                        {
                            _emailService.EnviarCorreo(
                                creador.correo,
                                "Notificación de subasta vendida",
                                mensajeCreador
                            );
                        }
                    }

                    // Notificar al ganador
                    if (!_context.notificaciones.Any(n => n.usuario_id == puj.usuario_id && n.mensaje == mensajeGanador))
                    {
                        _context.notificaciones.Add(new notificaciones
                        {
                            usuario_id = puj.usuario_id,
                            mensaje = mensajeGanador,
                            leido = false,
                            fecha = DateTime.UtcNow
                        });

                        var ganador = await _context.usuarios.FindAsync(puj.usuario_id);
                        if (ganador != null)
                        {
                            _emailService.EnviarCorreo(
                                ganador.correo,
                                "¡Felicidades! Ganaste la subasta",
                                mensajeGanador
                            );
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok("Subastas cerradas, notificaciones guardadas y correos enviados.");
        }

    }
}
