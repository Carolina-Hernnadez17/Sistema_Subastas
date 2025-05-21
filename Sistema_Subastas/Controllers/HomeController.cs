using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sistema_Subastas.Models;
using Sistema_Subastas.Services;
using System.Diagnostics;

namespace Sistema_Subastas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly subastaDbContext _context;
        private readonly IHubContext<NotificacionHub> _hubContext;
        private readonly EmailService _emailService;
        public HomeController(ILogger<HomeController> logger, subastaDbContext context, IHubContext<NotificacionHub> hubContext, EmailService emailService)
        {
            _logger = logger;
            _context = context;
            _hubContext = hubContext;
            _emailService = emailService;
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
            HoraServidor();

            var ahora = DateTime.Now;

            // Verificamos si hay alguna subasta que necesita ser cerrada
            bool hayPorFinalizar = _context.articulos
                .Any(a => a.estado_subasta == "Publicado" && a.fecha_fin <= ahora);

            if (hayPorFinalizar)
            {
                MarcarSubastasFinalizadas();
                DeterminarGanadores();
                await CerrarSubastasFinalizadas();
            }


            return View();
        }
        public IActionResult HoraServidor()
        {
            var horaLocal = DateTime.Now;
            var horaUtc = DateTime.UtcNow;

            return Content($"⏰ Hora Local del Servidor: {horaLocal:dd/MM/yyyy HH:mm:ss}\n" +
                           $"🌐 Hora UTC: {horaUtc:dd/MM/yyyy HH:mm:ss}");
        }


        public IActionResult Privacy()
        {
            return View();
        }
        //public void MarcarSubastasFinalizadas()
        //{
        //    var articulosParaFinalizar = _context.articulos
        //        .Where(a => a.estado_subasta == "Publicado" && a.fecha_fin <= DateTime.Now)
        //        .ToList();

        //    foreach (var articulo in articulosParaFinalizar)
        //    {
        //        articulo.estado_subasta = "Finalizada";
        //    }

        //    _context.SaveChanges();
        //}
        public void MarcarSubastasFinalizadas()
        {
            var ahora = DateTime.Now;
            var ahoraRedondeado = new DateTime(ahora.Year, ahora.Month, ahora.Day, ahora.Hour, ahora.Minute, ahora.Second);

            var articulosParaFinalizar = _context.articulos
                .Where(a => a.estado_subasta == "Publicado" &&
                            a.fecha_fin <= ahoraRedondeado)
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
                        fecha = DateTime.Now
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
                    string mensajeCreador = $"📢 Tu subasta {subasta.titulo} ha sido vendida. El ganador es el usuario con código: {puj.usuario_id}, monto final: {puj.monto}, finalizó el {subasta.fecha_fin:dd/MM/yyyy HH:mm}.";
                    string mensajeGanador = $"📢 Has ganado la subasta {subasta.titulo} (ID: {subasta.Id}), ¡felicidades!";

                    // Notificar al creador
                    if (!_context.notificaciones.Any(n => n.usuario_id == subasta.usuario_id && n.mensaje == mensajeCreador))
                    {
                        _context.notificaciones.Add(new notificaciones
                        {
                            usuario_id = subasta.usuario_id,
                            mensaje = mensajeCreador,
                            leido = false,
                            fecha = DateTime.Now
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
                            fecha = DateTime.Now
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
