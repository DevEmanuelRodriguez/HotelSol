using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HotelSol.Models;

[Table("PERSONA")]
public partial class Persona
{
    [Key]
    public int IdPersona { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string? TipoDocumento { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string? Documento { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Nombre { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Apellido { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Correo { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Clave { get; set; }

    public int? IdTipoPersona { get; set; }

    public bool? Estado { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [ForeignKey("IdTipoPersona")]
    [InverseProperty("Personas")]
    public virtual TipoPersona? IdTipoPersonaNavigation { get; set; }

    [InverseProperty("IdClienteNavigation")]
    public virtual ICollection<Recepcion> Recepcions { get; set; } = new List<Recepcion>();
}
