using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sistema_Subastas.Models;

namespace Sistema_Subastas.Controllers
{
    public class PujaejeController : Controller
    {

        private readonly subastaDbContext _context;
        private readonly IHubContext<NotificacionHub> _hubContext;
        public PujaejeController(subastaDbContext context, IHubContext<NotificacionHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: Pujas
        public async Task<IActionResult> Index()
        {
            return View(await _context.pujas.ToListAsync());
        }

        // GET: Pujas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pujas = await _context.pujas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pujas == null)
            {
                return NotFound();
            }

            return View(pujas);
        }

        // GET: Pujas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pujas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,articulo_id,usuario_id,monto,fecha_puja")] pujas pujas)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pujas);
                await _context.SaveChangesAsync();

                // Buscar el artículo
                var articulo = _context.articulos.FirstOrDefault(a => a.Id == pujas.articulo_id);
                if (articulo != null)
                {
                    // Crear la notificación
                    var notificacion = new notificaciones
                    {
                        usuario_id = articulo.usuario_id,
                        mensaje = $"📢 Nueva puja de ${pujas.monto} en tu subasta: {articulo.titulo}",
                        leido = false,
                        fecha = DateTime.Now
                    };
                    _context.notificaciones.Add(notificacion);
                    await _context.SaveChangesAsync();

                    // Enviar notificación en tiempo real
                    await _hubContext.Clients.All.SendAsync("RecibirNotificacion", articulo.usuario_id, notificacion.mensaje);
                }

                return View(pujas);
            }

            return View(pujas);
        }

    }
}
