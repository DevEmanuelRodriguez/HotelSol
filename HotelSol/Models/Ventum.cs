using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HotelSol.Models;

[Table("VENTA")]
public partial class Ventum
{
    [Key]
    public int IdVenta { get; set; }

    public int? IdRecepcion { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Total { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Estado { get; set; }

    [InverseProperty("IdVentaNavigation")]
    public virtual ICollection<DetalleVentum> DetalleVenta { get; set; } = new List<DetalleVentum>();

    [ForeignKey("IdRecepcion")]
    [InverseProperty("Venta")]
    public virtual Recepcion? IdRecepcionNavigation { get; set; }
}
