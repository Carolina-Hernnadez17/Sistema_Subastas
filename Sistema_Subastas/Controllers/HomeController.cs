using Microsoft.AspNetCore.Mvc;
using Sistema_Subastas.Models;
using System.Diagnostics;

namespace Sistema_Subastas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
