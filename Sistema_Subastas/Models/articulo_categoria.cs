using System.ComponentModel.DataAnnotations;

namespace Sistema_Subastas.Models
{
    public class articulo_categoria
    {
        [Key]

        public int articulo_id { get; set; }
        public int categoria_id { get; set; } = 0;
    }
}
