using Google.Protobuf.WellKnownTypes;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Subastas.Models
{
    public class pujas
    {
        [Key]
        public int Id { get; set; }
        public int articulo_id { get; set; }
        public int usuario_id { get; set; }
        public decimal monto { get; set; }
        public DateTime fecha_puja { get; set; }

        public string estado_pujas { get; set; } = "En espera";
    }
}
