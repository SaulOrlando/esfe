using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSocioEconomico.Domain.Entities;

public partial class Caso
{
    public int Id { get; set; }

    public int IdBeneficiado { get; set; }

    public string Titulo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string? ImagenUrl { get; set; }

    public decimal Meta { get; set; }

    public DateTime? FechaLimite { get; set; }

    public int IdCategoria { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<Donacione> Donaciones { get; set; } = new List<Donacione>();

    public virtual Usuario IdBeneficiadoNavigation { get; set; } = null!;

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual ICollection<Retiro> Retiros { get; set; } = new List<Retiro>();

    [NotMapped]
    public virtual ICollection<Programa> IdProgramas { get; set; } = new List<Programa>();
}
