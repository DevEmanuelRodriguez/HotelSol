using System;
using System.Collections.Generic;
using HotelSol.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelSol.Data;

public partial class DbHotelContext : DbContext
{
    //Constructores
    public DbHotelContext()
    {
    }

    public DbHotelContext(DbContextOptions<DbHotelContext> options)
        : base(options)
    {
    }

    //Cada DbSet es una tabla en la DB
    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<DetalleVentum> DetalleVenta { get; set; }

    public virtual DbSet<EstadoHabitacion> EstadoHabitacions { get; set; }

    public virtual DbSet<Habitacion> Habitacions { get; set; }

    public virtual DbSet<Persona> Personas { get; set; }

    public virtual DbSet<Piso> Pisos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Recepcion> Recepcions { get; set; }

    public virtual DbSet<TipoPersona> TipoPersonas { get; set; }

    public virtual DbSet<Ventum> Venta { get; set; }
    
    //Para configurar tablas
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {   //tabla categoria
        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__CATEGORI__A3C02A10A721BE6A");

            //atributos con valores por defecto
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<DetalleVentum>(entity =>
        {
            entity.HasKey(e => e.IdDetalleVenta).HasName("PK__DETALLE___AAA5CEC2B564843F");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleVenta).HasConstraintName("FK__DETALLE_V__IdPro__73BA3083");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.DetalleVenta).HasConstraintName("FK__DETALLE_V__IdVen__72C60C4A");
        });

        modelBuilder.Entity<EstadoHabitacion>(entity =>
        {
            entity.HasKey(e => e.IdEstadoHabitacion).HasName("PK__ESTADO_H__EBF610CE57457A10");

            entity.Property(e => e.IdEstadoHabitacion).ValueGeneratedNever();
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Habitacion>(entity =>
        {
            entity.HasKey(e => e.IdHabitacion).HasName("PK__HABITACI__8BBBF90110180FD9");

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Habitacions).HasConstraintName("FK__HABITACIO__IdCat__5812160E");

            entity.HasOne(d => d.IdEstadoHabitacionNavigation).WithMany(p => p.Habitacions).HasConstraintName("FK__HABITACIO__IdEst__5629CD9C");

            entity.HasOne(d => d.IdPisoNavigation).WithMany(p => p.Habitacions).HasConstraintName("FK__HABITACIO__IdPis__571DF1D5");
        });

        modelBuilder.Entity<Persona>(entity =>
        {
            entity.HasKey(e => e.IdPersona).HasName("PK__PERSONA__2EC8D2ACD797AF66");

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdTipoPersonaNavigation).WithMany(p => p.Personas).HasConstraintName("FK__PERSONA__IdTipoP__6477ECF3");
        });

        modelBuilder.Entity<Piso>(entity =>
        {
            entity.HasKey(e => e.IdPiso).HasName("PK__PISO__F2823D8A09257F5D");

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PK__PRODUCTO__09889210E2385899");

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Recepcion>(entity =>
        {
            entity.HasKey(e => e.IdRecepcion).HasName("PK__RECEPCIO__83F935CA77A5E313");

            entity.Property(e => e.CostoPenalidad).HasDefaultValue(0m);
            entity.Property(e => e.FechaEntrada).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TotalPagado).HasDefaultValue(0m);

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Recepcions).HasConstraintName("FK__RECEPCION__IdCli__693CA210");

            entity.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.Recepcions).HasConstraintName("FK__RECEPCION__IdHab__6A30C649");
        });

        modelBuilder.Entity<TipoPersona>(entity =>
        {
            entity.HasKey(e => e.IdTipoPersona).HasName("PK__TIPO_PER__79FCAFBF95963010");

            entity.Property(e => e.IdTipoPersona).ValueGeneratedNever();
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Ventum>(entity =>
        {
            entity.HasKey(e => e.IdVenta).HasName("PK__VENTA__BC1240BDC49D18CD");

            entity.HasOne(d => d.IdRecepcionNavigation).WithMany(p => p.Venta).HasConstraintName("FK__VENTA__IdRecepci__6FE99F9F");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
