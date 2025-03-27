namespace Sistema_Subastas.Models
{
    public class HistorialCompra
    {
        public int ArticuloId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string ImagenUrl { get; set; }
        public decimal PrecioFinal { get; set; }
        public DateTime FechaCierre { get; set; }
        public string EstadoTransaccion { get; set; }
    }
}
