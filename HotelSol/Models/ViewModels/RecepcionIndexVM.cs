using HotelSol.Models;

namespace HotelSol.Models.ViewModels
{
    // =====================================================
    // ViewModel para cada tarjeta de habitación
    // =====================================================
    public class HabitacionCardVM
    {
        // ID habitación
        public int IdHabitacion { get; set; }

        // Número habitación (101, 102, etc)
        public string Numero { get; set; } = "";

        // Categoría
        public string Categoria { get; set; } = "";

        // Piso
        public string Piso { get; set; } = "";

        // Texto visual
        // DISPONIBLE / RESERVADA / OCUPADA / LIMPIEZA
        public string EstadoTexto { get; set; } = "";

        // Clase CSS
        public string EstadoCss { get; set; } = "";

        // Puede crear nueva reserva
        public bool PuedeReservar { get; set; }

        // Compatibilidad con filtro por fechas
        public bool OcupadaEnFechas { get; set; }

        // Nueva reserva pendiente check-in
        public bool Reservada { get; set; }

        // Cliente hospedado actualmente
        public bool Ocupada { get; set; }

        // Id de recepción para botón Check-In
        public int IdRecepcion { get; set; }

        public bool PuedeCheckin { get; set; }
        public DateTime? FechaReserva { get; set; }
    }

    // =====================================================
    // ViewModel principal pantalla Recepción
    // =====================================================
    public class RecepcionIndexVM
    {
        // Piso seleccionado
        public int? PisoSeleccionado { get; set; }

        // Filtro fechas
        public DateTime? FechaEntrada { get; set; }

        public DateTime? FechaSalida { get; set; }

        // Combo pisos
        public List<Piso> Pisos { get; set; } = new();

        // Tarjetas habitaciones
        public List<HabitacionCardVM> Habitaciones { get; set; } = new();
    }
}