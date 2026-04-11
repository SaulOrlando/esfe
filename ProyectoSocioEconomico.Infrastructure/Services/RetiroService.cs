using Microsoft.EntityFrameworkCore;
using ProyectoSocioEconomico.Application.Interfaces;
using ProyectoSocioEconomico.Domain.Entities;
using ProyectoSocioEconomico.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSocioEconomico.Infrastructure.Services
{
    /// <summary>
    /// Servicio encargado de las solicitudes de retiro y cálculos
    /// de balance disponible por caso.
    /// </summary>
    public class RetiroService : IRetiroService
    {
        private readonly AppDbContext _context;

        public RetiroService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista los retiros de un beneficiario ordenados por fecha descendente.
        /// </summary>
        public async Task<List<Retiro>> ObtenerPorBeneficiadoIdAsync(int beneficiadoId)
        {
            return await _context.Retiros
                .Where(r => r.IdBeneficiado == beneficiadoId)
                .OrderByDescending(r => r.FechaSolicitud)
                .ToListAsync();
        }

        /// <summary>
        /// Devuelve todos los retiros con beneficiario y caso cargados
        /// para revisión administrativa.
        /// </summary>
        public async Task<List<Retiro>> ObtenerTodosConDetallesAsync()
        {
            return await _context.Retiros
                .Include(r => r.IdBeneficiadoNavigation)
                .Include(r => r.IdCasoNavigation)
                .OrderByDescending(r => r.FechaSolicitud)
                .ToListAsync();
        }

        /// <summary>
        /// Calcula el total retirado de un caso excluyendo retiros rechazados.
        /// </summary>
        public async Task<decimal> ObtenerTotalRetiradoPorCasoAsync(int casoId)
        {
            return await _context.Retiros
                .Where(r => r.IdCaso == casoId && r.Estado != "Rechazado")
                .SumAsync(r => r.Monto);
        }

        /// <summary>
        /// Calcula el saldo disponible del caso restando lo retirado
        /// del total de donaciones completadas.
        /// </summary>
        public async Task<decimal> ObtenerBalanceDisponibleAsync(int casoId)
        {
            var totalDonado = await _context.Donaciones
                .Where(d => d.IdCaso == casoId && d.Estado == "Completado")
                .SumAsync(d => d.Monto);

            var totalRetirado = await ObtenerTotalRetiradoPorCasoAsync(casoId);

            return totalDonado - totalRetirado;
        }

        /// <summary>
        /// Registra una nueva solicitud de retiro.
        /// </summary>
        public async Task Crear(Retiro retiro)
        {
            _context.Retiros.Add(retiro);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Actualiza el estado del retiro y registra la fecha de procesamiento.
        /// </summary>
        public async Task ActualizarEstadoAsync(int retiroId, string estado)
        {
            var retiro = await _context.Retiros.FirstOrDefaultAsync(r => r.Id == retiroId);
            if (retiro == null)
            {
                throw new InvalidOperationException("No se encontro la solicitud de retiro.");
            }

            retiro.Estado = estado;
            retiro.FechaProcesado = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}
