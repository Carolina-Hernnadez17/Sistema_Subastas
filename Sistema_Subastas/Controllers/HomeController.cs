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

        //public IActionResult Index()
        //{
        //    var usuario = HttpContext.Session.GetInt32("usuario_id");
        //    var nombre = HttpContext.Session.GetString("NombreUser");
        //    if(usuario == null && nombre == null)
        //    {
        //        return RedirectToAction("Login", "Usuarios");
        //    }
        //    ViewBag.NombreUsuario = nombre;
        //    CerrarSubastasFinalizadas();
        //    return View();
        //}
        public async Task<IActionResult> Index()
        {
            var usuario = HttpContext.Session.GetInt32("usuario_id");
            var nombre = HttpContext.Session.GetString("NombreUser");

            if (usuario == null && nombre == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            ViewBag.NombreUsuario = nombre;

            MarcarSubastasFinalizadas();
            await CerrarSubastasFinalizadas();
            DeterminarGanadores();

            // Aquí llamas al método async correctamente
            await CerrarSubastasFinalizadas();

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> CerrarSubastasFinalizadas()
        {
            // Subastas finalizadas con pujas
            var subastasFinalizadas = _context.articulos
                .Where(a => a.estado_subasta == "Finalizada")
                .ToList();

            foreach (var subasta in subastasFinalizadas)
            {
                string mensaje = $"📢 Tu subasta {subasta.titulo} ha finalizado el {subasta.fecha_fin:dd/MM/yyyy}.";

                // Verificar si ya se envió esta notificación
                bool yaExiste = _context.notificaciones.Any(n =>
                    n.usuario_id == subasta.usuario_id &&
                    n.mensaje == mensaje);

                if (!yaExiste)
                {
                    var notificacion = new notificaciones
                    {
                        usuario_id = subasta.usuario_id,
                        mensaje = mensaje,
                        leido = false,
                        fecha = DateTime.Now
                    };

                    _context.notificaciones.Add(notificacion);

                    //await _hubContext.Clients.All.SendAsync("RecibirNotificacion", subasta.usuario_id, mensaje);
                }
            }

            // Subastas no vendidas (sin pujas)
            var subastasNoVendidas = _context.articulos
                .Where(a => a.estado_subasta == "No vendido")
                .ToList();

            foreach (var subasta in subastasNoVendidas)
            {
                string mensaje = $"📢 Tu subasta {subasta.titulo} ha terminado sin pujas el {subasta.fecha_fin:dd/MM/yyyy}.";

                // Verificar si ya se envió esta notificación
                bool yaExiste = _context.notificaciones.Any(n =>
                    n.usuario_id == subasta.usuario_id &&
                    n.mensaje == mensaje);

                if (!yaExiste)
                {
                    var notificacion = new notificaciones
                    {
                        usuario_id = subasta.usuario_id,
                        mensaje = mensaje,
                        leido = false,
                        fecha = DateTime.Now
                    };

                    _context.notificaciones.Add(notificacion);

                   // await _hubContext.Clients.All.SendAsync("RecibirNotificacion", subasta.usuario_id, mensaje);
                }
            }
            // Subastas vendidas 
            var subastasVendidas = _context.articulos
                .Where(a => a.estado_subasta == "Vendido")
                .ToList();

            foreach (var subasta in subastasVendidas)
            {
                var pujas = _context.pujas
                .Where(a => a.articulo_id == subasta.Id)
                .ToList();

                foreach (var puj in pujas)
                {
                    if(puj.estado_pujas == "Ganador")
                    {
                        string mensaje = $"📢 Tu subasta {subasta.titulo} ha sido vendida  el ganador es el cod " +
                            $"de usuario: {puj.usuario_id} y el monto final es de: {puj.monto}, fecha de subasta finalizada: {subasta.fecha_fin:dd/MM/yyyy}.";

                        string mensaje2 = $"📢 Has sido ganador de la subasta {subasta.titulo} felicidades.";
                        // Verificar si ya se envió esta notificación
                        bool yaExiste = _context.notificaciones.Any(n =>
                            n.usuario_id == subasta.usuario_id &&
                            n.mensaje == mensaje);

                        if (!yaExiste)
                        {
                            var notificacion = new notificaciones
                            {
                                usuario_id = subasta.usuario_id,
                                mensaje = mensaje,
                                leido = false,
                                fecha = DateTime.Now
                            };
                            

                            _context.notificaciones.Add(notificacion);

                            // await _hubContext.Clients.All.SendAsync("RecibirNotificacion", subasta.usuario_id, mensaje);
                        }
                        bool yaExiste2 = _context.notificaciones.Any(n =>
                            n.usuario_id == puj.usuario_id &&
                            n.mensaje == mensaje2);

                        if (!yaExiste2) {

                            var notificacion2 = new notificaciones
                            {
                                usuario_id = puj.usuario_id,
                                mensaje = mensaje2,
                                leido = false,
                                fecha = DateTime.Now
                            };
                            _context.notificaciones.Add(notificacion2);
                        }
                    }

                }

            }

            await _context.SaveChangesAsync();

            return Ok("Subastas cerradas y notificaciones enviadas.");
        }
        public void MarcarSubastasFinalizadas()
        {
            var articulosParaFinalizar = _context.articulos
                .Where(a => a.estado_subasta == "Publicado" && a.fecha_fin <= DateTime.Now)
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




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
