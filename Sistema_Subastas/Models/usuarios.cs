using Google.Protobuf.WellKnownTypes;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Subastas.Models
{
    public class usuarios
    {
        [Key]
        public int id { get; set; }

        [Display(Name = "Nombre usuario")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
        public string nombre { get; set; }

        [Display(Name = "Apellido")]
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido no puede tener más de 50 caracteres.")]
        public string apellido { get; set; }

        [Display(Name = "Correo")]
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [RegularExpression(
            @"^[a-zA-Z0-9._%+-]+@(gmail\.com|hotmail\.com|outlook\.com|yahoo\.com|icloud\.com|protonmail\.com|aol\.com|zoho\.com)$",
            ErrorMessage = "El correo electrónico debe ser de un dominio permitido (gmail.com, hotmail.com, outlook.com, etc.)."
        )]
        public string correo { get; set; }

        [Display(Name = "Teléfono")]
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^\+\d{1,3}\s?\d{6,14}$", ErrorMessage = "Número de teléfono no válido.")]
        public string telefono { get; set; }

        [Display(Name = "Dirección")]
        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(100, ErrorMessage = "La dirección no puede tener más de 100 caracteres.")]
        public string direccion { get; set; }

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string contrasena { get; set; }

        [Display(Name = "Fecha de registro")]
        public DateTime fecha_registro { get; set; } = DateTime.Now;

        public bool Estado { get; set; } = true;

        public bool TipoUser { get; set; } = true;




    }
}
