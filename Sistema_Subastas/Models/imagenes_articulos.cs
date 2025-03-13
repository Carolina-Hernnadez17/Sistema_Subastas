using System.ComponentModel.DataAnnotations;

namespace Sistema_Subastas.Models
{
    public class imagenes_articulos
    {
        [Key]

        public int id { get; set; }
        public int articulo_id { get; set; }
        public string url_imagen {  get; set; }
    }
}
