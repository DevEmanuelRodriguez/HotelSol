using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HotelSol.Models;

[Table("TIPO_PERSONA")]
public partial class TipoPersona
{
    [Key]
    public int IdTipoPersona { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Descripcion { get; set; }

    public bool? Estado { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [InverseProperty("IdTipoPersonaNavigation")]
    public virtual ICollection<Persona> Personas { get; set; } = new List<Persona>();
}
