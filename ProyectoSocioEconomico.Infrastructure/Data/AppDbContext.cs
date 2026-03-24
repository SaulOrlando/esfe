using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ProyectoSocioEconomico.Domain.Entities;

namespace ProyectoSocioEconomico.Infrastructure.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Caso> Casos { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Comprobante> Comprobantes { get; set; }

    public virtual DbSet<Donacione> Donaciones { get; set; }

    public virtual DbSet<InscripcionesVoluntario> InscripcionesVoluntarios { get; set; }

    public virtual DbSet<LogsFinanciero> LogsFinancieros { get; set; }

    public virtual DbSet<Notificacione> Notificaciones { get; set; }

    public virtual DbSet<Programa> Programas { get; set; }

    public virtual DbSet<Retiro> Retiros { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=ProyectoSocioeconomicoDB;Trusted_Connection=True;Encrypt=False;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Caso>(entity =>
        {
            entity.HasIndex(e => e.IdBeneficiado, "IX_Casos_IdBeneficiado");

            entity.HasIndex(e => e.IdCategoria, "IX_Casos_IdCategoria");

            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.Meta).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Titulo).HasMaxLength(150);

            entity.HasOne(d => d.IdBeneficiadoNavigation).WithMany(p => p.Casos)
                .HasForeignKey(d => d.IdBeneficiado)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Casos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasMany(d => d.IdProgramas).WithMany(p => p.IdCasos)
                .UsingEntity<Dictionary<string, object>>(
                    "CasosPrograma",
                    r => r.HasOne<Programa>().WithMany()
                        .HasForeignKey("IdPrograma")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    l => l.HasOne<Caso>().WithMany()
                        .HasForeignKey("IdCaso")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey("IdCaso", "IdPrograma");
                        j.ToTable("CasosProgramas");
                        j.HasIndex(new[] { "IdPrograma" }, "IX_CasosProgramas_IdPrograma");
                    });
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Comprobante>(entity =>
        {
            entity.HasIndex(e => e.DonacionId, "IX_Comprobantes_DonacionId");

            entity.Property(e => e.CodigoComprobante).HasDefaultValue("");

            entity.HasOne(d => d.Donacion).WithMany(p => p.Comprobantes)
                .HasForeignKey(d => d.DonacionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Donacione>(entity =>
        {
            entity.HasIndex(e => e.IdCaso, "IX_Donaciones_IdCaso");

            entity.HasIndex(e => e.IdDonador, "IX_Donaciones_IdDonador");

            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.MetodoPago).HasMaxLength(50);
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCasoNavigation).WithMany(p => p.Donaciones)
                .HasForeignKey(d => d.IdCaso)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IdDonadorNavigation).WithMany(p => p.Donaciones)
                .HasForeignKey(d => d.IdDonador)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<InscripcionesVoluntario>(entity =>
        {
            entity.HasIndex(e => e.IdPrograma, "IX_InscripcionesVoluntarios_IdPrograma");

            entity.HasIndex(e => e.IdUsuario, "IX_InscripcionesVoluntarios_IdUsuario");

            entity.Property(e => e.Estado).HasMaxLength(20);

            entity.HasOne(d => d.IdProgramaNavigation).WithMany(p => p.InscripcionesVoluntarios)
                .HasForeignKey(d => d.IdPrograma)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.InscripcionesVoluntarios)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<LogsFinanciero>(entity =>
        {
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Notificacione>(entity =>
        {
            entity.HasIndex(e => e.UsuarioId, "IX_Notificaciones_UsuarioId");

            entity.Property(e => e.Tipo).HasDefaultValue("");
            entity.Property(e => e.Titulo).HasDefaultValue("");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Programa>(entity =>
        {
            entity.HasIndex(e => e.CreadoPor, "IX_Programas_CreadoPor");

            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.Nombre).HasMaxLength(150);

            entity.HasOne(d => d.CreadoPorNavigation).WithMany(p => p.Programas)
                .HasForeignKey(d => d.CreadoPor)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Retiro>(entity =>
        {
            entity.HasIndex(e => e.IdBeneficiado, "IX_Retiros_IdBeneficiado");

            entity.HasIndex(e => e.IdCaso, "IX_Retiros_IdCaso");

            entity.Property(e => e.DatosPago).HasMaxLength(255);
            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.MetodoPago).HasMaxLength(50);
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdBeneficiadoNavigation).WithMany(p => p.Retiros)
                .HasForeignKey(d => d.IdBeneficiado)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IdCasoNavigation).WithMany(p => p.Retiros)
                .HasForeignKey(d => d.IdCaso)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Descripcion).HasMaxLength(150);
            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(e => e.IdRol, "IX_Usuarios_IdRol");

            entity.Property(e => e.Apellido).HasMaxLength(100);
            entity.Property(e => e.Direccion).HasMaxLength(255);
            entity.Property(e => e.Dui)
                .HasMaxLength(20)
                .HasColumnName("DUI");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Telefono).HasMaxLength(20);

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
