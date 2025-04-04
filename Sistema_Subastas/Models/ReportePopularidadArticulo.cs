namespace Sistema_Subastas.Models
{
    public class ReportePopularidadArticulo
    {
        public string TituloObra { get; set; }
        public string NombreArtista { get; set; }
        public int CantidadPujas { get; set; }
        public decimal MontoMaximoPuja { get; set; }
        public DateTime FechaUltimaPuja { get; set; }
    }
}
