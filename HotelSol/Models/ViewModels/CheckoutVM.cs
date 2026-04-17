using HotelSol.Models;

namespace HotelSol.Models.ViewModels
{
    public class CheckoutVM
    {
        public int IdRecepcion { get; set; }
        public int IdHabitacion { get; set; }

        public string NumeroHabitacion { get; set; } = "";
        public string DetalleHabitacion { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Piso { get; set; } = "";

        public string Cliente { get; set; } = "";
        public string Documento { get; set; } = "";
        public string Correo { get; set; } = "";

        public DateTime? FechaEntrada { get; set; }
        public DateTime? FechaSalida { get; set; }

        public decimal CostoHabitacion { get; set; }
        public decimal Adelanto { get; set; }
        public decimal CantidadRestante { get; set; }
        public decimal Penalidad { get; set; }

        public decimal TotalConsumos { get; set; }
        public decimal TotalPagar { get; set; }

        public List<RecepcionDetalleProductoVM> Productos { get; set; } = new();
    }
}