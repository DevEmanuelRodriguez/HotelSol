using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HotelSol.Models;

[Table("RECEPCION")]
public partial class Recepcion
{
    [Key]
    public int IdRecepcion { get; set; }

    public int? IdCliente { get; set; }

    public int? IdHabitacion { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaEntrada { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaSalida { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaSalidaConfirmacion { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? PrecioInicial { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Adelanto { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? PrecioRestante { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? TotalPagado { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? CostoPenalidad { get; set; }

    [StringLength(500)]
    [Unicode(false)]
    public string? Observacion { get; set; }

    public bool? Estado { get; set; }

    [ForeignKey("IdCliente")]
    [InverseProperty("Recepcions")]
    public virtual Persona? IdClienteNavigation { get; set; }

    [ForeignKey("IdHabitacion")]
    [InverseProperty("Recepcions")]
    public virtual Habitacion? IdHabitacionNavigation { get; set; }

    [InverseProperty("IdRecepcionNavigation")]
    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}
