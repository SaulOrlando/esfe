using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSocioEconomico.Domain.Entities;

public partial class Programa
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string? ImagenUrl { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public decimal MetaFinanciera { get; set; }

    public int IdCategoria { get; set; }

    public int CreadoPor { get; set; }

    public virtual Usuario CreadoPorNavigation { get; set; } = null!;

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual ICollection<InscripcionesVoluntario> InscripcionesVoluntarios { get; set; } = new List<InscripcionesVoluntario>();

    [NotMapped]
    public virtual ICollection<Caso> IdCasos { get; set; } = new List<Caso>();
}
