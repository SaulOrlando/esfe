using ProyectoSocioEconomico.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSocioEconomico.Application.Interfaces
{
    public interface IProgramaService
    {
        Task<List<Programa>> ObtenerTodosConCasosAsync();
        Task<Programa?> ObtenerPorIdConDetallesAsync(int id);
        Task<List<InscripcionesVoluntario>> ObtenerInscripcionesVoluntariadoPorProgramaAsync(int programaId);
        Task<List<InscripcionesVoluntario>> ObtenerSolicitudesVoluntariadoAsync();
        Task<InscripcionesVoluntario?> ObtenerInscripcionActivaPorUsuarioAsync(int usuarioId);
        Task AprobarInscripcionVoluntariadoAsync(int inscripcionId);
        Task RechazarInscripcionVoluntariadoAsync(int inscripcionId);
        Task ActualizarDisponibilidadVoluntarioAsync(int usuarioId, IEnumerable<string> diasDisponibles);
        Task SalirDelProgramaAsync(int usuarioId);
        Task CrearAsync(Programa programa);
        Task ActualizarAsync(Programa programa);
        Task SincronizarEstadoPorMetaAsync(int programaId);
        Task EliminarAsync(int id);
    }
}
