using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HotelSol.Models;

[Table("ESTADO_HABITACION")]
public partial class EstadoHabitacion
{
    [Key]
    public int IdEstadoHabitacion { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Descripcion { get; set; }

    public bool? Estado { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [InverseProperty("IdEstadoHabitacionNavigation")]
    public virtual ICollection<Habitacion> Habitacions { get; set; } = new List<Habitacion>();
}
