using Microsoft.EntityFrameworkCore;
using ProyectoSocioEconomico.Application.Interfaces;
using ProyectoSocioEconomico.Domain.Entities;
using ProyectoSocioEconomico.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSocioEconomico.Infrastructure.Services
{
    public class CasoService : ICasoService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public CasoService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Caso>> ObtenerTodos()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Casos
                .Include(c => c.IdCategoriaNavigation)
                .Include(c => c.IdBeneficiadoNavigation)
                .ToListAsync();
        }

        public async Task<List<Caso>> ObtenerTodosConDetallesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Casos
                .Include(c => c.IdCategoriaNavigation)
                .Include(c => c.IdBeneficiadoNavigation)
                .Include(c => c.Donaciones)
                .ToListAsync();
        }

        public async Task<List<Caso>> ObtenerActivosParaHome(int cantidad)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Casos
                .Include(c => c.Donaciones)
                .Where(c => c.Estado == "Activo")
                .OrderByDescending(c => c.FechaCreacion)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<Caso?> ObtenerPorIdConDetallesAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Casos
                .Include(c => c.IdCategoriaNavigation)
                .Include(c => c.IdBeneficiadoNavigation)
                .Include(c => c.Donaciones)
                    .ThenInclude(d => d.IdDonadorNavigation)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Caso?> ObtenerPorBeneficiadoIdAsync(int usuarioId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Casos
                .Include(c => c.IdCategoriaNavigation)
                .Include(c => c.IdBeneficiadoNavigation)
                .Include(c => c.Donaciones)
                .OrderByDescending(c => c.FechaCreacion)
                .FirstOrDefaultAsync(c => c.IdBeneficiado == usuarioId);
        }

        public async Task<bool> UsuarioYaTieneCasoAsync(int usuarioId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Casos.AnyAsync(c => c.IdBeneficiado == usuarioId);
        }

        public async Task Crear(Caso caso)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var usuario = await context.Usuarios.FindAsync(caso.IdBeneficiado);
            if (usuario != null)
            {
                var rolBeneficiario = await context.Roles
                    .FirstOrDefaultAsync(r =>
                        r.Nombre.ToLower() == "beneficiario" ||
                        r.Nombre.ToLower() == "beneficiado");

                if (rolBeneficiario == null)
                {
                    throw new InvalidOperationException("No se encontró el rol Beneficiario en la base de datos.");
                }

                if (usuario.IdRol != rolBeneficiario.Id)
                {
                    usuario.IdRol = rolBeneficiario.Id;
                }
            }

            context.Casos.Add(caso);
            await context.SaveChangesAsync();
        }

        public async Task Actualizar(Caso caso)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var casoExistente = await context.Casos.FirstOrDefaultAsync(c => c.Id == caso.Id);
            if (casoExistente == null)
            {
                throw new InvalidOperationException("No se encontró el caso a actualizar.");
            }

            casoExistente.Titulo = caso.Titulo;
            casoExistente.Descripcion = caso.Descripcion;
            casoExistente.IdCategoria = caso.IdCategoria;
            casoExistente.Estado = caso.Estado;
            casoExistente.ImagenUrl = caso.ImagenUrl;
            casoExistente.FechaLimite = caso.FechaLimite;

            await context.SaveChangesAsync();
        }

        public async Task SincronizarEstadoPorMetaAsync(int casoId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var casoExistente = await context.Casos.FirstOrDefaultAsync(c => c.Id == casoId);
            if (casoExistente == null)
            {
                return;
            }

            var totalRecaudado = await context.Donaciones
                .Where(d => d.IdCaso == casoId && d.Estado == "Completado")
                .SumAsync(d => d.Monto);

            if (totalRecaudado >= casoExistente.Meta)
            {
                casoExistente.Estado = "Inactivo";
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<Categoria>> ObtenerCategorias()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Categorias
                .Where(c => c.Estado == "Activo")
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }
    }
}
