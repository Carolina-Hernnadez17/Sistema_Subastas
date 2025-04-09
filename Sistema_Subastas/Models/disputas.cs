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
        [Required(ErrorMessage = "Este campo es obligatorio")]

        public string motivo {  get; set; }
        [Required(ErrorMessage = "Este campo es obligatorio")]

        public string descripcion { get; set; }
        [Required(ErrorMessage = "Este campo es obligatorio")]
        [RegularExpression("Abierta|En proceso|Resuelta|Rechazada", ErrorMessage = "Estado inválido")]
        public string estado { get; set; }

        [Required(ErrorMessage = "Este campo es obligatorio")]

        public DateTime fecha { get; set; }
    }
}
