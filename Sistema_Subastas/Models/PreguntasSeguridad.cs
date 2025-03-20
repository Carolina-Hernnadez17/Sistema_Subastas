using System.ComponentModel.DataAnnotations;

namespace Sistema_Subastas.Models
{
    public class PreguntasSeguridad
    {
        [Key]
        public int id { get; set; }
        public int user_id { get; set; }
        public string question { get; set; }
        public string answer { get; set; }
    }

}
