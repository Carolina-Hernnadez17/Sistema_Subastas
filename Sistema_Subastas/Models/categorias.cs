using System.ComponentModel.DataAnnotations;

namespace Sistema_Subastas.Models
{
    public class categorias
    {
        [Key]
        public int Id { get; set; }
        public string nombre { get; set; }
    }
}
