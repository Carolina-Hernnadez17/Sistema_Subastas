using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sistema_Subastas.Models;
using System.Diagnostics;

namespace Sistema_Subastas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly subastaDbContext _context;
        private readonly IHubContext<NotificacionHub> _hubContext;
        public HomeController(ILogger<HomeController> logger, subastaDbContext context, IHubContext<NotificacionHub> hubContext)
        {
            _logger = logger;
            _context = context;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            var usuario = HttpContext.Session.GetInt32("usuario_id");
            var nombre = HttpContext.Session.GetString("NombreUser");
            if(usuario == null && nombre == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }
            ViewBag.NombreUsuario = nombre;
            CerrarSubastasFinalizadas();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> CerrarSubastasFinalizadas()
        {
            // Obtener las subastas cerradas
            var subastas = _context.articulos
                .Where(a => a.estado_subasta == "Finalizada")
                .ToList();

            foreach (var subasta in subastas)
            {
                string mensaje;

                mensaje = $"Tu subasta '{subasta.titulo}' ha finalizado.";

                // Crear la notificación para el creador de la subasta
                var notificacion = new notificaciones
                {
                    usuario_id = subasta.usuario_id,
                    mensaje = mensaje,
                    leido = false,
                    fecha = DateTime.Now
                };

                // Agregar la notificación a la base de datos
                _context.notificaciones.Add(notificacion);

                // Enviar la notificación en tiempo real (por SignalR)
                await _hubContext.Clients.All.SendAsync("RecibirNotificacion", subasta.usuario_id, mensaje);
            }

            // Obtener las subastas cerradas
            var subastasNV = _context.articulos
                .Where(a => a.estado_subasta == "No vendido")
                .ToList();

            foreach (var subastaN in subastasNV)
            {
                string mensaje;

                mensaje = $"Tu subasta '{subastaN.titulo}' ha terminado sin pujas .";

                // Crear la notificación para el creador de la subasta
                var notificacion = new notificaciones
                {
                    usuario_id = subastaN.usuario_id,
                    mensaje = mensaje,
                    leido = false,
                    fecha = DateTime.Now
                };

                // Agregar la notificación a la base de datos
                _context.notificaciones.Add(notificacion);

                // Enviar la notificación en tiempo real (por SignalR)
                await _hubContext.Clients.All.SendAsync("RecibirNotificacion", subastaN.usuario_id, mensaje);
            }

            await _context.SaveChangesAsync();

            return Ok("Subastas cerradas y notificaciones enviadas.");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
