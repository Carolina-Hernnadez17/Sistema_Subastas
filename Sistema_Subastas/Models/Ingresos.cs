namespace Sistema_Subastas.Models
{
    public class Ingresos
    {
        public string Titulo { get; set; }
        public DateTime FechaVenta { get; set; }
        public decimal PrecioFinal { get; set; }
        public decimal Comision { get; set; }
        public decimal IngresoNeto { get; set; }
    }
}
