using System.ComponentModel.DataAnnotations;
using HotelSol.Models;

namespace HotelSol.Models.ViewModels
{
    public class RecepcionCreateVM
    {
        // ID HABITACIÓN
        public int IdHabitacion { get; set; }

        // DATOS VISUALES HABITACIÓN
        public string NumeroHabitacion { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Piso { get; set; } = "";
        public string DetalleHabitacion { get; set; } = "";

        // PRECIO BASE POR NOCHE
        public decimal PrecioHabitacion { get; set; }

        // LISTA DE CLIENTES
        public List<Persona> Clientes { get; set; } = new();

        // BUSCADOR
        [Display(Name = "Nro Documento")]
        public string? DocumentoBusqueda { get; set; }

        // CLIENTE SELECCIONADO
        public int? IdClienteExistente { get; set; }

        // DATOS CLIENTE
        [Required(ErrorMessage = "Seleccione tipo de documento")]
        public string? TipoDocumento { get; set; }

        [Required(ErrorMessage = "Ingrese documento")]
        public string? Documento { get; set; }

        [Required(ErrorMessage = "Ingrese nombre")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "Ingrese apellido")]
        public string? Apellido { get; set; }

        [Required(ErrorMessage = "Ingrese correo")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string? Correo { get; set; }

        // FECHAS
        [Required(ErrorMessage = "Seleccione fecha de entrada")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? FechaEntrada { get; set; }

        [Required(ErrorMessage = "Seleccione fecha de salida")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? FechaSalida { get; set; }

        // PRECIO TOTAL CALCULADO
        public decimal? PrecioInicial { get; set; }

        // PAGOS
        public decimal? Adelanto { get; set; }

        // OBSERVACIONES
        public string? Observacion { get; set; }
    }
}