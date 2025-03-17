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
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("id,nombre,apellido,correo,telefono,direccion,contrasena,fecha_registro,estado")] usuarios usuarios)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(usuarios);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    // Obtener el ID del usuario recién insertado
        //    string getUserIdQuery = "SELECT LAST_INSERT_ID()";
        //    MySqlCommand getUserIdCmd = new MySqlCommand(getUserIdQuery, conn);
        //    int userId = Convert.ToInt32(getUserIdCmd.ExecuteScalar());

        //    return Json(new { success = true, userId = userId });
        //    return View(usuarios);
        //}

        public async Task<IActionResult> Create([Bind("id,nombre,apellido,correo,telefono,direccion,contrasena,fecha_registro,estado")] usuarios usuarios)
        {
            if (ModelState.IsValid)
            {
                _context.Add(usuarios);
                await _context.SaveChangesAsync(); // Guarda los cambios en la BD

                // Obtener el ID del usuario recién insertado
                int userId = usuarios.id; // Como `id` es clave primaria con autoincrement, EF lo asigna automáticamente

                return Json(new { success = true, userId = userId });
            }

            return View(usuarios);
        }

        // Acción para mostrar el formulario de pregunta de seguridad
        public IActionResult PreguntasSeguridad(int userId)
        {
            var preguntas = new List<string>
            {
                "¿Cuál es tu color favorito?",
                "¿En qué ciudad naciste?",
                "¿Cuál es el nombre de tu primera mascota?",
                "¿Cuál es tu comida favorita?"
            };
            var preguntas2 = new List<string>
            {
                "¿Cuál es el nombre de tu abuela materna?",
                "¿Cuál fue tu primer trabajo?",
                "¿Cuál es tu película favorita?",
                "¿Cuál era el nombre de tu peluche favorito?"
            };

            var viewModel = new PreguntasSeguridad
            {
                UserId = userId
            };

            ViewBag.Preguntas = preguntas;
            ViewBag.Preguntas2 = preguntas2;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarPreguntasSeguridad(int userId, string question1, string answer1, string question2, string answer2)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Crear la primera instancia de PreguntasSeguridad
                    var pregunta1 = new PreguntasSeguridad
                    {
                        UserId = userId,
                        Question = question1,  // Primera pregunta
                        Answer = answer1       // Primera respuesta
                    };

                    // Crear la segunda instancia de PreguntasSeguridad
                    var pregunta2 = new PreguntasSeguridad
                    {
                        UserId = userId,
                        Question = question2,  // Segunda pregunta
                        Answer = answer2       // Segunda respuesta
                    };

                    // Agregar las preguntas a la base de datos
                    _context.PreguntasSeguridad.AddRange(pregunta1, pregunta2);
                    await _context.SaveChangesAsync(); // Guardar en la base de datos usando Entity Framework

                    // Redirigir al login después de guardar
                    return RedirectToAction("Login", "Usuarios");
                }
                catch (Exception ex)
                {
                    // Si hay un error, mostrar el mensaje de error
                    ViewBag.Error = "Error al guardar las respuestas: " + ex.Message;
                    return View("PreguntasSeguridad");  // Volver a la vista con el error
                }
            }

            // Si el modelo no es válido, volver a cargar las preguntas
            ViewBag.Preguntas = new List<string>
            {
                "¿Cuál es tu color favorito?",
                "¿En qué ciudad naciste?",
                "¿Cuál es el nombre de tu primera mascota?",
                "¿Cuál es tu comida favorita?"
            };
            ViewBag.Preguntas2 = new List<string>
            {
                "¿Cuál es el nombre de tu abuela materna?",
                "¿Cuál fue tu primer trabajo?",
                "¿Cuál es tu película favorita?",
                "¿Cuál era el nombre de tu peluche favorito?"
            };

            return View("PreguntasSeguridad"); // Si hay error, se vuelve a mostrar el formulario
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
