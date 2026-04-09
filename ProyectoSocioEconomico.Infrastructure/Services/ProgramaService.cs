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
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.CreadoPorNavigation)
                .Include(p => p.Donaciones)
                .Include(p => p.InscripcionesVoluntarios)
                    .ThenInclude(i => i.IdUsuarioNavigation)
                .FirstOrDefaultAsync(p => p.Id == id);
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
            currentProgram.ImagenUrl = programa.ImagenUrl;

            await context.SaveChangesAsync();
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
