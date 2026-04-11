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
    public class ProgramaService : IProgramaService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ProgramaService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

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

        public async Task CrearAsync(Programa programa)
        {
            ArgumentNullException.ThrowIfNull(programa);

            using var context = await _contextFactory.CreateDbContextAsync();
            context.Programas.Add(programa);
            await context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Programa programa)
        {
            ArgumentNullException.ThrowIfNull(programa);

            using var context = await _contextFactory.CreateDbContextAsync();
            var currentProgram = await context.Programas.FirstOrDefaultAsync(p => p.Id == programa.Id);

            if (currentProgram is null)
            {
                throw new InvalidOperationException("No se encontró el programa a actualizar.");
            }

            currentProgram.Nombre = programa.Nombre;
            currentProgram.Descripcion = programa.Descripcion;
            currentProgram.Estado = programa.Estado;
            currentProgram.TipoPrograma = programa.TipoPrograma;
            currentProgram.IdCategoria = programa.IdCategoria;
            currentProgram.MetaFinanciera = programa.MetaFinanciera;
            currentProgram.MetaVoluntarios = programa.MetaVoluntarios;
            currentProgram.DiasVoluntariado = programa.DiasVoluntariado;
            currentProgram.ImagenUrl = programa.ImagenUrl;

            await context.SaveChangesAsync();
        }

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
                await context.SaveChangesAsync();
            }
        }

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
                context.InscripcionesVoluntarios.RemoveRange(programa.InscripcionesVoluntarios);
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
    }
}
