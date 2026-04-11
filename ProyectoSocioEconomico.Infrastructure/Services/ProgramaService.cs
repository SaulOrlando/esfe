using Microsoft.EntityFrameworkCore;
using ProyectoSocioEconomico.Application.Interfaces;
using ProyectoSocioEconomico.Domain.Entities;
using ProyectoSocioEconomico.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSocioEconomico.Infrastructure.Services
{
    /// <summary>
    /// Servicio de negocio para programas institucionales y flujo de voluntariado.
    /// Aquí viven las reglas más importantes sobre metas, inscripciones y roles.
    /// </summary>
    public class ProgramaService : IProgramaService
    {
        private static readonly string[] VolunteerApprovedStates = { "Aprobado", "Activo" };
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ProgramaService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Devuelve todos los programas con categoría, creador, donaciones
        /// e inscripciones cargadas para vistas administrativas.
        /// </summary>
        public async Task<List<Programa>> ObtenerTodosConCasosAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Programas
                .AsSplitQuery()
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.CreadoPorNavigation)
                .Include(p => p.Donaciones)
                .Include(p => p.InscripcionesVoluntarios)
                .ToListAsync();
        }

        /// <summary>
        /// Devuelve únicamente los programas activos visibles al público.
        /// </summary>
        public async Task<List<Programa>> ObtenerPublicosConCasosAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Programas
                .AsSplitQuery()
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.CreadoPorNavigation)
                .Include(p => p.Donaciones)
                .Include(p => p.InscripcionesVoluntarios)
                .Where(p => p.Estado == "Activo")
                .ToListAsync();
        }

        /// <summary>
        /// Busca un programa con todos los detalles necesarios para administración
        /// y revisión de voluntarios.
        /// </summary>
        public async Task<Programa?> ObtenerPorIdConDetallesAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Programas
                .AsSplitQuery()
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.CreadoPorNavigation)
                .Include(p => p.Donaciones)
                .Include(p => p.InscripcionesVoluntarios)
                    .ThenInclude(i => i.IdUsuarioNavigation)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Busca un programa público por id, restringiendo resultados
        /// a programas activos.
        /// </summary>
        public async Task<Programa?> ObtenerPublicoPorIdConDetallesAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Programas
                .AsSplitQuery()
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.CreadoPorNavigation)
                .Include(p => p.Donaciones)
                .Include(p => p.InscripcionesVoluntarios)
                    .ThenInclude(i => i.IdUsuarioNavigation)
                .FirstOrDefaultAsync(p => p.Id == id && p.Estado == "Activo");
        }

        /// <summary>
        /// Obtiene todas las inscripciones de un programa ordenadas
        /// desde la más reciente.
        /// </summary>
        public async Task<List<InscripcionesVoluntario>> ObtenerInscripcionesVoluntariadoPorProgramaAsync(int programaId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.InscripcionesVoluntarios
                .AsNoTracking()
                .Include(i => i.IdUsuarioNavigation)
                .Where(i => i.IdPrograma == programaId)
                .OrderByDescending(i => i.FechaInscripcion)
                .ToListAsync();
        }

        /// <summary>
        /// Devuelve las solicitudes globales de voluntariado, priorizando
        /// las pendientes para facilitar la revisión administrativa.
        /// </summary>
        public async Task<List<InscripcionesVoluntario>> ObtenerSolicitudesVoluntariadoAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.InscripcionesVoluntarios
                .AsNoTracking()
                .Include(i => i.IdUsuarioNavigation)
                .Include(i => i.IdProgramaNavigation)
                .ThenInclude(p => p.IdCategoriaNavigation)
                .OrderBy(i => i.Estado == "Pendiente" ? 0 : 1)
                .ThenByDescending(i => i.FechaInscripcion)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene la inscripción activa de voluntariado de un usuario,
        /// junto con los detalles del programa asociado.
        /// </summary>
        public async Task<InscripcionesVoluntario?> ObtenerInscripcionActivaPorUsuarioAsync(int usuarioId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.InscripcionesVoluntarios
                .AsSplitQuery()
                .Include(i => i.IdUsuarioNavigation)
                    .ThenInclude(u => u.IdRolNavigation)
                .Include(i => i.IdProgramaNavigation)
                    .ThenInclude(p => p.IdCategoriaNavigation)
                .Include(i => i.IdProgramaNavigation)
                    .ThenInclude(p => p.CreadoPorNavigation)
                .Include(i => i.IdProgramaNavigation)
                    .ThenInclude(p => p.Donaciones)
                .Include(i => i.IdProgramaNavigation)
                    .ThenInclude(p => p.InscripcionesVoluntarios)
                .Where(i =>
                    i.IdUsuario == usuarioId &&
                    (i.Estado == "Aprobado" || i.Estado == "Activo"))
                .OrderByDescending(i => i.FechaInscripcion)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Aprueba una postulación si cumple reglas de negocio:
        /// cupos disponibles, rol permitido y ausencia de conflicto con casos.
        /// </summary>
        public async Task AprobarInscripcionVoluntariadoAsync(int inscripcionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var inscripcion = await context.InscripcionesVoluntarios
                .Include(i => i.IdProgramaNavigation)
                .FirstOrDefaultAsync(i => i.Id == inscripcionId);

            if (inscripcion is null)
            {
                throw new InvalidOperationException("No se encontró la postulación de voluntariado.");
            }

            if (string.Equals(inscripcion.Estado, "Rechazado", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("No se puede aprobar una postulación ya rechazada.");
            }

            var aprobadasActuales = await context.InscripcionesVoluntarios.CountAsync(i =>
                i.IdPrograma == inscripcion.IdPrograma &&
                i.Id != inscripcion.Id &&
                (i.Estado == "Aprobado" || i.Estado == "Activo"));

            if (inscripcion.IdProgramaNavigation.MetaVoluntarios > 0 &&
                aprobadasActuales >= inscripcion.IdProgramaNavigation.MetaVoluntarios)
            {
                throw new InvalidOperationException("La meta de voluntarios del programa ya fue alcanzada.");
            }

            var volunteerRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Nombre == "Voluntario");

            if (volunteerRole is null)
            {
                throw new InvalidOperationException("No existe el rol de voluntario en la base de datos.");
            }

            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == inscripcion.IdUsuario);
            if (usuario is null)
            {
                throw new InvalidOperationException("No se encontró el usuario asociado a la postulación.");
            }

            var beneficiaryRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Nombre == "Beneficiario");

            var usuarioYaTieneCaso = await context.Casos.AnyAsync(c => c.IdBeneficiado == inscripcion.IdUsuario);
            var usuarioEsBeneficiario = beneficiaryRole is not null && usuario.IdRol == beneficiaryRole.Id;

            if (usuarioYaTieneCaso || usuarioEsBeneficiario)
            {
                throw new InvalidOperationException("Un usuario beneficiario o con un caso creado no puede ser aprobado como voluntario.");
            }

            inscripcion.Estado = "Aprobado";
            usuario.IdRol = volunteerRole.Id;

            var otrasPendientes = await context.InscripcionesVoluntarios
                .Where(i =>
                    i.IdUsuario == inscripcion.IdUsuario &&
                    i.Id != inscripcion.Id &&
                    i.Estado == "Pendiente")
                .ToListAsync();

            foreach (var solicitud in otrasPendientes)
            {
                solicitud.Estado = "Rechazado";
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Marca una postulación como rechazada.
        /// </summary>
        public async Task RechazarInscripcionVoluntariadoAsync(int inscripcionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var inscripcion = await context.InscripcionesVoluntarios
                .FirstOrDefaultAsync(i => i.Id == inscripcionId);

            if (inscripcion is null)
            {
                throw new InvalidOperationException("No se encontró la postulación de voluntariado.");
            }

            inscripcion.Estado = "Rechazado";
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Remueve a un voluntario aprobado/activo por incumplimiento
        /// y restablece su rol si ya no participa en otros programas activos.
        /// </summary>
        public async Task RemoverVoluntarioPorIncumplimientoAsync(int inscripcionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var inscripcion = await context.InscripcionesVoluntarios
                .FirstOrDefaultAsync(i => i.Id == inscripcionId);

            if (inscripcion is null)
            {
                throw new InvalidOperationException("No se encontro la inscripcion de voluntariado.");
            }

            if (!VolunteerApprovedStates.Contains(inscripcion.Estado, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Solo se puede quitar el rol a voluntarios aprobados o activos.");
            }

            inscripcion.Estado = "Removido";
            await context.SaveChangesAsync();
            await RestablecerRolDonanteSiCorrespondeAsync(context, new[] { inscripcion.IdUsuario });
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Actualiza los días disponibles del voluntario validando
        /// que estén permitidos por el programa.
        /// </summary>
        public async Task ActualizarDisponibilidadVoluntarioAsync(int usuarioId, IEnumerable<string> diasDisponibles)
        {
            ArgumentNullException.ThrowIfNull(diasDisponibles);

            using var context = await _contextFactory.CreateDbContextAsync();

            var inscripcion = await context.InscripcionesVoluntarios
                .Include(i => i.IdProgramaNavigation)
                .FirstOrDefaultAsync(i =>
                    i.IdUsuario == usuarioId &&
                    (i.Estado == "Aprobado" || i.Estado == "Activo"));

            if (inscripcion is null)
            {
                throw new InvalidOperationException("No se encontró una inscripción activa de voluntariado para este usuario.");
            }

            var diasPrograma = ParseDays(inscripcion.IdProgramaNavigation.DiasVoluntariado);
            if (diasPrograma.Count == 0)
            {
                throw new InvalidOperationException("El programa no tiene días de voluntariado configurados.");
            }

            var diasSeleccionados = diasDisponibles
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => d.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (diasSeleccionados.Count == 0)
            {
                throw new InvalidOperationException("Debes seleccionar al menos un día disponible.");
            }

            var diasInvalidos = diasSeleccionados
                .Where(d => !diasPrograma.Contains(d, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (diasInvalidos.Count > 0)
            {
                throw new InvalidOperationException("Seleccionaste días que no están disponibles en el programa.");
            }

            inscripcion.DiasDisponibles = string.Join(", ",
                diasPrograma.Where(d => diasSeleccionados.Contains(d, StringComparer.OrdinalIgnoreCase)));

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Elimina la inscripción activa del usuario en un programa
        /// y restituye su rol si aplica.
        /// </summary>
        public async Task SalirDelProgramaAsync(int usuarioId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var inscripcion = await context.InscripcionesVoluntarios
                .FirstOrDefaultAsync(i =>
                    i.IdUsuario == usuarioId &&
                    VolunteerApprovedStates.Contains(i.Estado, StringComparer.OrdinalIgnoreCase));

            if (inscripcion is null)
            {
                throw new InvalidOperationException("No se encontró una inscripción activa de voluntariado para este usuario.");
            }

            context.InscripcionesVoluntarios.Remove(inscripcion);
            await context.SaveChangesAsync();
            await RestablecerRolDonanteSiCorrespondeAsync(context, new[] { usuarioId });
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Crea un programa nuevo.
        /// </summary>
        public async Task CrearAsync(Programa programa)
        {
            ArgumentNullException.ThrowIfNull(programa);

            using var context = await _contextFactory.CreateDbContextAsync();
            context.Programas.Add(programa);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Actualiza el programa. Si cambia a inactivo, también finaliza
        /// las inscripciones activas y reevalúa los roles de usuarios afectados.
        /// </summary>
        public async Task ActualizarAsync(Programa programa)
        {
            ArgumentNullException.ThrowIfNull(programa);

            using var context = await _contextFactory.CreateDbContextAsync();
            var currentProgram = await context.Programas.FirstOrDefaultAsync(p => p.Id == programa.Id);

            if (currentProgram is null)
            {
                throw new InvalidOperationException("No se encontró el programa a actualizar.");
            }

            var changedToInactive =
                !string.Equals(currentProgram.Estado, "Inactivo", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(programa.Estado, "Inactivo", StringComparison.OrdinalIgnoreCase);

            currentProgram.Nombre = programa.Nombre;
            currentProgram.Descripcion = programa.Descripcion;
            currentProgram.Estado = programa.Estado;
            currentProgram.TipoPrograma = programa.TipoPrograma;
            currentProgram.IdCategoria = programa.IdCategoria;
            currentProgram.MetaFinanciera = programa.MetaFinanciera;
            currentProgram.MetaVoluntarios = programa.MetaVoluntarios;
            currentProgram.DiasVoluntariado = programa.DiasVoluntariado;
            currentProgram.ImagenUrl = programa.ImagenUrl;

            if (changedToInactive)
            {
                var affectedUserIds = await FinalizarVoluntariosDeProgramaInactivoAsync(context, currentProgram.Id);
                await context.SaveChangesAsync();
                await RestablecerRolDonanteSiCorrespondeAsync(context, affectedUserIds);
                await context.SaveChangesAsync();
                return;
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Inactiva el programa automáticamente si alcanza su meta financiera.
        /// Además finaliza voluntarios y reevalúa sus roles cuando corresponde.
        /// </summary>
        public async Task SincronizarEstadoPorMetaAsync(int programaId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var programaExistente = await context.Programas.FirstOrDefaultAsync(p => p.Id == programaId);
            if (programaExistente is null)
            {
                return;
            }

            var aceptaMetaFinanciera =
                string.Equals(programaExistente.TipoPrograma, "Financiero", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(programaExistente.TipoPrograma, "Hibrido", StringComparison.OrdinalIgnoreCase);

            if (!aceptaMetaFinanciera || programaExistente.MetaFinanciera <= 0)
            {
                return;
            }

            var totalRecaudado = await context.Donaciones
                .Where(d => d.IdPrograma == programaId && d.Estado == "Completado")
                .SumAsync(d => d.Monto);

            if (totalRecaudado >= programaExistente.MetaFinanciera)
            {
                programaExistente.Estado = "Inactivo";
                var affectedUserIds = await FinalizarVoluntariosDeProgramaInactivoAsync(context, programaExistente.Id);
                await context.SaveChangesAsync();
                await RestablecerRolDonanteSiCorrespondeAsync(context, affectedUserIds);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Elimina un programa, limpia inscripciones y donaciones asociadas,
        /// y corrige roles de voluntarios afectados.
        /// </summary>
        public async Task EliminarAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var programa = await context.Programas
                .Include(p => p.Donaciones)
                .Include(p => p.InscripcionesVoluntarios)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (programa is null)
            {
                return;
            }

            if (programa.InscripcionesVoluntarios.Any())
            {
                var volunteerUserIds = programa.InscripcionesVoluntarios
                    .Where(i => VolunteerApprovedStates.Contains(i.Estado))
                    .Select(i => i.IdUsuario)
                    .Distinct()
                    .ToList();

                context.InscripcionesVoluntarios.RemoveRange(programa.InscripcionesVoluntarios);
                await context.SaveChangesAsync();
                await RestablecerRolDonanteSiCorrespondeAsync(context, volunteerUserIds);
            }

            if (programa.Donaciones.Any())
            {
                foreach (var donacion in programa.Donaciones)
                {
                    donacion.IdPrograma = null;
                }
            }

            context.Programas.Remove(programa);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Cambia a "Finalizado" las inscripciones activas de un programa
        /// que dejó de estar disponible.
        /// </summary>
        private static async Task<List<int>> FinalizarVoluntariosDeProgramaInactivoAsync(AppDbContext context, int programaId)
        {
            var inscripcionesAfectadas = await context.InscripcionesVoluntarios
                .Where(i => i.IdPrograma == programaId &&
                            VolunteerApprovedStates.Contains(i.Estado))
                .ToListAsync();

            if (inscripcionesAfectadas.Count == 0)
            {
                return new List<int>();
            }

            foreach (var inscripcion in inscripcionesAfectadas)
            {
                inscripcion.Estado = "Finalizado";
            }

            return inscripcionesAfectadas
                .Select(i => i.IdUsuario)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Devuelve al rol de donante a quienes ya no conservan
        /// ninguna participación activa como voluntarios.
        /// </summary>
        private static async Task RestablecerRolDonanteSiCorrespondeAsync(AppDbContext context, IEnumerable<int> userIds)
        {
            var userIdList = userIds
                .Distinct()
                .ToList();

            if (userIdList.Count == 0)
            {
                return;
            }

            var volunteerRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Nombre == "Voluntario");

            var donorRole = await context.Roles
                .FirstOrDefaultAsync(r =>
                    string.Equals(r.Nombre, "Donante", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(r.Nombre, "Donador", StringComparison.OrdinalIgnoreCase));

            if (volunteerRole is null || donorRole is null)
            {
                throw new InvalidOperationException("No se encontraron los roles requeridos para actualizar al usuario.");
            }

            var usuarios = await context.Usuarios
                .Where(u => userIdList.Contains(u.Id) && u.IdRol == volunteerRole.Id)
                .ToListAsync();

            foreach (var usuario in usuarios)
            {
                var conservaVoluntariadoActivo = await context.InscripcionesVoluntarios
                    .AnyAsync(i =>
                        i.IdUsuario == usuario.Id &&
                        VolunteerApprovedStates.Contains(i.Estado) &&
                        i.IdProgramaNavigation.Estado == "Activo");

                if (!conservaVoluntariadoActivo)
                {
                    usuario.IdRol = donorRole.Id;
                }
            }
        }

        /// <summary>
        /// Convierte una cadena separada por comas en una lista normalizada
        /// de días, evitando duplicados.
        /// </summary>
        private static List<string> ParseDays(string? rawDays)
        {
            return string.IsNullOrWhiteSpace(rawDays)
                ? new List<string>()
                : rawDays
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
        }
    }
}
