using System.ComponentModel.DataAnnotations;

namespace Sistema_Subastas.Models
{
    public class seguimiento_subastas
    {
        [Key]
        public int id { get; set; }
        public int usuario_id { get; set; }
        public int articulo_id { get; set; }
    }
}
