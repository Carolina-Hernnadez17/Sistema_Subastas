using Google.Protobuf.WellKnownTypes;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Subastas.Models
{
    public class usuarios
    {
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
        public string nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido no puede tener más de 50 caracteres.")]
        public string apellido { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [RegularExpression(
            @"^[a-zA-Z0-9._%+-]+@(gmail\.com|hotmail\.com|outlook\.com|yahoo\.com|icloud\.com|protonmail\.com|aol\.com|zoho\.com)$",
            ErrorMessage = "El correo electrónico debe ser de un dominio permitido (gmail.com, hotmail.com, outlook.com, etc.)."
        )]
        public string correo { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^\+\d{1,3}\s?\d{6,14}$", ErrorMessage = "Número de teléfono no válido.")]
        public string telefono { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(100, ErrorMessage = "La dirección no puede tener más de 100 caracteres.")]
        public string direccion { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string contrasena { get; set; }

        public DateTime fecha_registro { get; set; } = DateTime.Now;

        public bool Estado { get; set; } = true;


    }
}
