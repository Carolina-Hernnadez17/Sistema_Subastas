using System.ComponentModel.DataAnnotations;

namespace Sistema_Subastas.Models
{
    public class articulos
    {
        [Key]
        public int Id { get; set; }

        public string titulo { get; set; }
        public string descripcion { get; set; }
        public string estado { get; set; }

        public decimal precio_salida { get; set; }
        public decimal precio_venta { get; set; }

        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_fin {  get; set; }
        public int usuario_id { get; set; }

        public string visualizacion_puja { get; set; }

        public string estado_subasta { get; set; } = "Publicado";
        public DateTime fecha_registro { get; set; } = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "America/El_Salvador");
    }
}
