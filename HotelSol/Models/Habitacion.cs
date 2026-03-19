using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HotelSol.Models;

[Table("HABITACION")]
public partial class Habitacion
{
    [Key]
    public int IdHabitacion { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Numero { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Detalle { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Precio { get; set; }

    public int? IdEstadoHabitacion { get; set; }

    public int? IdPiso { get; set; }

    public int? IdCategoria { get; set; }

    public bool? Estado { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [ForeignKey("IdCategoria")]
    [InverseProperty("Habitacions")]
    public virtual Categorium? IdCategoriaNavigation { get; set; }

    [ForeignKey("IdEstadoHabitacion")]
    [InverseProperty("Habitacions")]
    public virtual EstadoHabitacion? IdEstadoHabitacionNavigation { get; set; }

    [ForeignKey("IdPiso")]
    [InverseProperty("Habitacions")]
    public virtual Piso? IdPisoNavigation { get; set; }

    [InverseProperty("IdHabitacionNavigation")]
    public virtual ICollection<Recepcion> Recepcions { get; set; } = new List<Recepcion>();
}
