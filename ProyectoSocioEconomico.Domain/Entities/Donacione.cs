using System;
using System.Collections.Generic;

namespace ProyectoSocioEconomico.Domain.Entities;

public partial class Donacione
{
    public int Id { get; set; }

    public int? IdCaso { get; set; }

    public int? IdPrograma { get; set; }

    public int IdDonador { get; set; }

    public decimal Monto { get; set; }

    public bool Anonimo { get; set; }

    public DateTime FechaDonacion { get; set; }

    public string Estado { get; set; } = null!;

    public string MetodoPago { get; set; } = null!;

    public virtual ICollection<Comprobante> Comprobantes { get; set; } = new List<Comprobante>();

    public virtual Caso? IdCasoNavigation { get; set; }

    public virtual Programa? IdProgramaNavigation { get; set; }

    public virtual Usuario IdDonadorNavigation { get; set; } = null!;
}
