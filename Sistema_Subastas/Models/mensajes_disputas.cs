using Google.Protobuf.WellKnownTypes;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Subastas.Models
{
    public class mensajes_disputas
    {
        [Key]
        public int id { get; set; }
        public int desputa_id { get; set; }
        public int usuario_id { get; set; }
        public string mensaje { get; set; }
        public DateTime fecha { get; set; }
    }
}
