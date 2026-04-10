using ProyectoSocioEconomico.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSocioEconomico.Application.Interfaces
{
    public interface ICasoService
    {
        Task<List<Caso>> ObtenerTodos();
        Task<List<Caso>> ObtenerTodosConDetallesAsync();
        Task<List<Caso>> ObtenerActivosParaHome(int cantidad);
        Task<Caso?> ObtenerPorIdConDetallesAsync(int id);
        Task<Caso?> ObtenerPorBeneficiadoIdAsync(int usuarioId);
        Task<bool> UsuarioYaTieneCasoAsync(int usuarioId);
        Task Crear(Caso caso);
        Task Actualizar(Caso caso);
        Task EliminarAsync(int id);
        Task SincronizarEstadoPorMetaAsync(int casoId);
        Task<List<Categoria>> ObtenerCategorias();
    }
}
