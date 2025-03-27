using System;

namespace Sistema_Subastas.Models
{
    public class HistorialVenta
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        public string ImagenUrl { get; set; }

        public decimal PrecioFinal { get; set; }
        public DateTime FechaCierre { get; set; }
        public string EstadoTransaccion { get; set; }

        public string Categoria { get; set; }
    }
}