namespace Sistema_Subastas.Models
{
    public class ReporteArticulosViewModel
    {
        public List<Articulo> ReportePorPujas { get; set; }
        public List<Articulo> ReportePorVistas { get; set; }
    }

    public class Articulo
    {
        public string TituloObra { get; set; }
        public string NombreArtista { get; set; }
        public int CantidadPujas { get; set; }
        public decimal MontoMaximoPuja { get; set; }
        public DateTime FechaUltimaPuja { get; set; }
    }
}
