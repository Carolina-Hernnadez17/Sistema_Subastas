using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;

namespace Sistema_Subastas.Models
{
    public class subastaDbContext : DbContext
    {
        public subastaDbContext(DbContextOptions<subastaDbContext> options) : base(options) { }

        public DbSet<articulo_categoria> articulo_categoria{ get; set; }
        public DbSet<articulos> articulos { get; set; }
        public DbSet<categorias> categorias { get; set; }
        public DbSet<disputas> disputas { get; set; }
        public DbSet<historial> historial { get; set; }
        public DbSet<imagenes_articulos> imagenes_articulos { get; set; }
        public DbSet<mensajes_disputas> mensajes_disputas { get; set; }
        public DbSet<notificaciones> notificaciones { get; set; }
        public DbSet<pujas> pujas { get; set; }
        public DbSet<seguimiento_subastas> seguimiento_subastas { get; set; }
        public DbSet<usuarios> usuarios { get; set; }
        public DbSet<valoraciones> valoraciones { get; set; }
    }
}
