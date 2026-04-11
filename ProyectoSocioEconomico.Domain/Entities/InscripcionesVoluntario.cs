using System;
using System.Collections.Generic;

namespace ProyectoSocioEconomico.Domain.Entities;

public partial class InscripcionesVoluntario
{
    public int Id { get; set; }

    public int IdPrograma { get; set; }

    public int IdUsuario { get; set; }

    public DateTime FechaInscripcion { get; set; }

    public string CategoriaVoluntariado { get; set; } = null!;

    public string DiasDisponibles { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public virtual Programa IdProgramaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
