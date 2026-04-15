using HotelSol.Models;

namespace HotelSol.Models.ViewModels
{
    public class TiendaVentaVM
    {
        public int IdHabitacion { get; set; }
        public int IdRecepcion { get; set; }

        public string NumeroHabitacion { get; set; } = "";
        public string Cliente { get; set; } = "";
        public string Documento { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Piso { get; set; } = "";

        public DateTime? FechaEntrada { get; set; }

        // PRODUCTOS
        public List<Producto> Productos { get; set; } = new();
    }
}