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
    /// Servicio de negocio para la gestión de casos solidarios.
    /// Encapsula lectura, escritura y sincronización de estados.
    /// </summary>
    public class CasoService : ICasoService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public CasoService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Obtiene todos los casos con su categoría y beneficiario.
        /// Ideal para listados generales.
        /// </summary>
        public async Task<List<Caso>> ObtenerTodos()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Casos
                .Include(c => c.IdCategoriaNavigation)
                .Include(c => c.IdBeneficiadoNavigation)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene los casos con información ampliada, incluyendo donaciones,
        /// para pantallas que muestran progreso financiero.
        /// </summary>
        public async Task<List<Caso>> ObtenerTodosConDetallesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Casos
                .Include(c => c.IdCategoriaNavigation)
                .Include(c => c.IdBeneficiadoNavigation)
                .Include(c => c.Donaciones)
                .ToListAsync();
        }

        /// <summary>
        /// Devuelve una cantidad limitada de casos activos ordenados por fecha
        /// de creación descendente para la página principal.
        /// </summary>
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

        /// <summary>
        /// Busca un caso por id y carga las relaciones necesarias para
        /// vistas de detalle, incluyendo donadores.
        /// </summary>
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

        /// <summary>
        /// Devuelve el caso más reciente asociado a un beneficiario.
        /// Se usa para dashboards y navegación contextual.
        /// </summary>
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

        /// <summary>
        /// Determina si el usuario ya tiene un caso registrado.
        /// Esta regla evita duplicar campañas por beneficiario.
        /// </summary>
        public async Task<bool> UsuarioYaTieneCasoAsync(int usuarioId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Casos.AnyAsync(c => c.IdBeneficiado == usuarioId);
        }

        /// <summary>
        /// Crea un caso nuevo y, si es necesario, cambia el rol del usuario
        /// al rol de beneficiario antes de persistirlo.
        /// </summary>
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

        /// <summary>
        /// Actualiza solamente los campos editables del caso.
        /// No reemplaza la entidad completa para evitar sobrescribir relaciones.
        /// </summary>
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

        /// <summary>
        /// Elimina un caso y resuelve sus efectos colaterales:
        /// desvincula donaciones, elimina retiros y reevalúa el rol del usuario.
        /// </summary>
        public async Task EliminarAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var casoExistente = await context.Casos
                .Include(c => c.Donaciones)
                .Include(c => c.Retiros)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (casoExistente == null)
            {
                throw new InvalidOperationException("No se encontro el caso a eliminar.");
            }

            foreach (var donacion in casoExistente.Donaciones)
            {
                donacion.IdCaso = null;
            }

            if (casoExistente.Retiros.Any())
            {
                context.Retiros.RemoveRange(casoExistente.Retiros);
            }

            var rolDonante = await context.Roles
                .FirstOrDefaultAsync(r => r.Nombre.ToLower() == "donante");

            if (rolDonante == null)
            {
                throw new InvalidOperationException("No se encontro el rol Donante en la base de datos.");
            }

            var usuario = await context.Usuarios.FindAsync(casoExistente.IdBeneficiado);
            if (usuario != null && usuario.IdRol != rolDonante.Id)
            {
                usuario.IdRol = rolDonante.Id;
            }

            context.Casos.Remove(casoExistente);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Si las donaciones completadas alcanzan la meta del caso,
        /// marca el caso como inactivo.
        /// </summary>
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

        /// <summary>
        /// Devuelve solo las categorías activas ordenadas alfabéticamente.
        /// </summary>
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
