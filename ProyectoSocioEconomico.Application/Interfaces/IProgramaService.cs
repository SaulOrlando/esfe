using ProyectoSocioEconomico.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSocioEconomico.Application.Interfaces
{
    public interface IProgramaService
    {
        Task<List<Programa>> ObtenerTodosConCasosAsync();
        Task<Programa?> ObtenerPorIdConDetallesAsync(int id);
        Task CrearAsync(Programa programa);
        Task ActualizarAsync(Programa programa);
        Task SincronizarEstadoPorMetaAsync(int programaId);
        Task EliminarAsync(int id);
    }
}
