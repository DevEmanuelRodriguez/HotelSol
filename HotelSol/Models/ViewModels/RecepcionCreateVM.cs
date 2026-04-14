using System.ComponentModel.DataAnnotations;
using HotelSol.Models;

namespace HotelSol.Models.ViewModels
{
    public class RecepcionCreateVM
    {
        public int IdHabitacion { get; set; }

        public string NumeroHabitacion { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Piso { get; set; } = "";
        public string DetalleHabitacion { get; set; } = "";
        public decimal PrecioHabitacion { get; set; }

        [Display(Name = "Nro Documento")]
        public string? DocumentoBusqueda { get; set; }

        public int? IdClienteExistente { get; set; }

        [Required]
        public string? TipoDocumento { get; set; }

        [Required]
        public string? Documento { get; set; }

        [Required]
        public string? Nombre { get; set; }

        [Required]
        public string? Apellido { get; set; }

        [Required]
        [EmailAddress]
        public string? Correo { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? FechaEntrada { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? FechaSalida { get; set; }

        [Required]
        public decimal? PrecioInicial { get; set; }

        public decimal? Adelanto { get; set; }

        public string? Observacion { get; set; }

        public List<TipoPersona> TiposPersona { get; set; } = new();
    }
}