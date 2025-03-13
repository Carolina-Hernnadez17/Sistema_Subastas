using Google.Protobuf.WellKnownTypes;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Subastas.Models
{
    public class notificaciones
    {
        [Key]
        public int id { get; set; }
        public int usuario_id { get; set; }
        public string mensaje { get; set; }
        public Boolean leido { get; set; }
        public DateTime fecha { get; set; }
    }
}
