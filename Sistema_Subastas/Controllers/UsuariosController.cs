using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Sistema_Subastas.Models;

namespace Sistema_Subastas.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly subastaDbContext _context;

        public UsuariosController(subastaDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(usuarios usuario)
        {
            try
            {
                var user = _context.usuarios.FirstOrDefault(u => u.correo == usuario.correo && u.Estado == true);

                if (user == null)
                {
                    TempData["Mensaje"] = "Cuenta inexistente o cerrada.";
                    return RedirectToAction("Login");
                }

                // Comparar contraseñas directamente (sin hash)
                if (user.contrasena != usuario.contrasena)
                {
                    TempData["Mensaje"] = "Correo o contraseña incorrectos.";
                    return RedirectToAction("Login");
                }
                HttpContext.Session.SetInt32("id_usuario", user.id);
                HttpContext.Session.SetString("NombreUser", user.nombre);

                TempData["UserId"] = user.id;
                return RedirectToAction("Index", "Home");

            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error al realizar el login: " + ex.Message;
                return View();
            }
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            return View(await _context.usuarios.ToListAsync());
        }

        //GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            //var usuarioId = HttpContext.Session.GetInt32("usuario_id");
            //var jiji = HttpContext.
            ////var nombre = HttpContext.Session.GetString("NombreUser");
            //if (usuarioId == null)
            //{
            //    return RedirectToAction("Login", "Usuarios");
            //}
            if (id == null)
            {
                return NotFound();
            }
            
            var usuarios = await _context.usuarios
                .FirstOrDefaultAsync(m => m.id == id);
            if (usuarios == null)
            {
                return NotFound();
            }

            return View(usuarios);
        }


        // GET: Usuarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("id,nombre,apellido,correo,telefono,direccion,contrasena,fecha_registro,estado")] usuarios usuarios)
        //{
        //    try
        //    {

        //        if (ModelState.IsValid)
        //        {
        //            if (usuarios.correo != null)
        //            {
        //                ViewBag.Error = "El correo ya existe ingrese uno nuevo.";

        //            }
        //            _context.Add(usuarios);
        //            await _context.SaveChangesAsync(); // Guarda los cambios en la BD

        //            // Obtener el ID del usuario recién insertado
        //            int userId = usuarios.id; // Como `id` es clave primaria con autoincrement, EF lo asigna automáticamente

        //            return Json(new { success = true, userId = userId });
        //        }

        //        return View(usuarios);
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Mensaje = "Error al realizar el guardado de datos: " + ex.Message;
        //        return View();
        //    }

        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,nombre,apellido,correo,telefono,direccion,contrasena,fecha_registro,estado")] usuarios usuarios)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar si el correo ya existe en la base de datos
                    bool correoExiste = await _context.usuarios.AnyAsync(u => u.correo == usuarios.correo);
                    if (correoExiste)
                    {
                        ModelState.AddModelError("correo", "El correo ya existe, ingrese uno nuevo.");
                        return Json(new { success = false, message = "El correo ya existe, ingrese uno nuevo." });
                    }

                    _context.Add(usuarios);
                    await _context.SaveChangesAsync(); // Guarda los cambios en la BD

                    // Obtener el ID del usuario recién insertado
                    int userId = usuarios.id;

                    return Json(new { success = true, userId = userId });
                }

                return Json(new { success = false, message = "Datos inválidos, revise el formulario." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al realizar el guardado de datos: " + ex.Message });
            }
        }



        [HttpGet]
        public IActionResult PreguntasSeguridad()
        {
            return View();
        }


        [HttpPost]
        public IActionResult GuardarPreguntas(int userId, string question1, string answer1, string question2, string answer2)
        {
            var pregunta1 = new PreguntasSeguridad
            {
                //id = 1,
                user_id = userId,
                question = question1,
                answer = answer1
            };
            _context.Add(pregunta1);
            _context.SaveChanges();

            var pregunta2 = new PreguntasSeguridad
            {
               // id = 2,
                user_id = userId,
                question = question2,
                answer = answer2
            };
            _context.Add(pregunta2);
            _context.SaveChanges();

            return RedirectToAction("Index", "Home"); // Redirige después de guardar
        }
        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarios = await _context.usuarios.FindAsync(id);
            if (usuarios == null)
            {
                return NotFound();
            }
            return View(usuarios);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,nombre,apellido,correo,telefono,direccion,contrasena,fecha_registro,estado")] usuarios usuarios)
        {
            if (id != usuarios.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuarios);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!usuariosExists(usuarios.id))
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
            return View(usuarios);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarios = await _context.usuarios
                .FirstOrDefaultAsync(m => m.id == id);
            if (usuarios == null)
            {
                return NotFound();
            }

            return View(usuarios);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuarios = await _context.usuarios.FindAsync(id);
            if (usuarios != null)
            {
                _context.usuarios.Remove(usuarios);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool usuariosExists(int id)
        {
            return _context.usuarios.Any(e => e.id == id);
        }
    }
}
