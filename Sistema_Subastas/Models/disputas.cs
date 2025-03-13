using Google.Protobuf.WellKnownTypes;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Subastas.Models
{
    public class disputas
    {
        [Key]
        public int id { get; set; }
        public int articulo_id { get; set; }
        public int comprador_id { get; set; }
        public int vendedor_id { get; set; }
        public string motivo {  get; set; }
        public string descripcion { get; set; }
        public string estado { get; set; }
        public DateTime fecha { get; set; }
    }
}
