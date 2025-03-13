using Google.Protobuf.WellKnownTypes;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Subastas.Models
{
    public class historial
    {
        [Key]
        public int Id { get; set; }
        public int usuario_id { get; set; }
        public int articulo_id { get; set; }
        public string tipo { get; set; }
        public string estado { get; set; }
        public DateTime fecha { get; set; }
    }
}
