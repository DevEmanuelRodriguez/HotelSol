using HotelSol.Models;

namespace HotelSol.Models.ViewModels
{
    public class HabitacionCardVM
    {
        public int IdHabitacion { get; set; }
        public string Numero { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Piso { get; set; } = "";
        public string EstadoTexto { get; set; } = "";
        public string EstadoCss { get; set; } = "";
        public bool PuedeReservar { get; set; }
    }

    public class RecepcionIndexVM
    {
        public int? PisoSeleccionado { get; set; }
        public List<Piso> Pisos { get; set; } = new();
        public List<HabitacionCardVM> Habitaciones { get; set; } = new();
    }
}