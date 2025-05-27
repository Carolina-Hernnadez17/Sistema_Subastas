using Google.Protobuf.WellKnownTypes;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Subastas.Models
{
    public class valoraciones
    {
        [Key]
        public int id { get; set; }
        public int usuario_valorado_id{ get; set; }
        public int usuario_que_valora_id{ get; set; }
        public int puntuacion { get; set; }
        public string comentario { get; set; }
        public DateTime fecha { get; set; } = DateTime.Now;
    }
}
