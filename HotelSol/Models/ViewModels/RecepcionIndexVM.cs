using HotelSol.Models;

namespace HotelSol.Models.ViewModels
{
    // ViewModel para cada tarjeta de habitación
    public class HabitacionCardVM
    {
        // ID de la habitación (clave primaria)
        public int IdHabitacion { get; set; }

        // Número de la habitación (ej: 101)
        public string Numero { get; set; } = "";

        // Categoría (Simple, Doble, Suite, etc.)
        public string Categoria { get; set; } = "";

        // Piso (Primero, Segundo, etc.)
        public string Piso { get; set; } = "";

        // Texto que se muestra en la tarjeta
        public string EstadoTexto { get; set; } = "";

        // Clase CSS para color visual
        
        public string EstadoCss { get; set; } = "";

        // Indica si se puede hacer clic (permitir reserva)
        public bool PuedeReservar { get; set; }

        
        // Indica si está ocupada en el rango de fechas seleccionado
        // (esto será clave para disponibilidad real)
        public bool OcupadaEnFechas { get; set; }

    }

    // ViewModel principal de la vista Recepción (Index)
    public class RecepcionIndexVM
    {
        // Piso seleccionado en el filtro
        public int? PisoSeleccionado { get; set; }

        // Fecha de entrada seleccionada
        // Permite filtrar disponibilidad
        public DateTime? FechaEntrada { get; set; }

        // Fecha de salida seleccionada
        public DateTime? FechaSalida { get; set; }

        // Lista de pisos para el dropdown
        public List<Piso> Pisos { get; set; } = new();

        // Lista de habitaciones (tarjetas)
        public List<HabitacionCardVM> Habitaciones { get; set; } = new();
    }
}