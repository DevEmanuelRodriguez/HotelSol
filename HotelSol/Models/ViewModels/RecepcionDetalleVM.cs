using HotelSol.Models;

namespace HotelSol.Models.ViewModels
{
    public class RecepcionDetalleProductoVM
    {
        public string Producto { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string EstadoVenta { get; set; } = "";
        public decimal SubTotal { get; set; }
    }

    public class RecepcionDetalleVM
    {
        // DATOS HABITACIÓN
        public int IdHabitacion { get; set; }
        public string NumeroHabitacion { get; set; } = "";
        public string DetalleHabitacion { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Piso { get; set; } = "";

        // DATOS CLIENTE
        public int IdCliente { get; set; }
        public string Cliente { get; set; } = "";
        public string Documento { get; set; } = "";
        public string Correo { get; set; } = "";

        // DATOS HOSPEDAJE
        public int IdRecepcion { get; set; }
        public DateTime? FechaEntrada { get; set; }
        public DateTime? FechaSalida { get; set; }
        public decimal CostoHabitacion { get; set; }
        public decimal CantidadAdelantado { get; set; }
        public decimal CantidadRestante { get; set; }

        // SERVICIOS / PRODUCTOS
        public List<RecepcionDetalleProductoVM> Productos { get; set; } = new();
    }
}