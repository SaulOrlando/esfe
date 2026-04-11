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
    /// Servicio encargado de las operaciones de usuario:
    /// consulta, creación, actualización y validación de credenciales.
    /// </summary>
    public class UsuarioService : IUsuarioService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public UsuarioService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Devuelve todos los usuarios sin cargar relaciones adicionales.
        /// Se usa para listados simples.
        /// </summary>
        public async Task<List<Usuario>> ObtenerTodos()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Usuarios.ToListAsync();
        }

        /// <summary>
        /// Busca un usuario por id incluyendo su rol para que la UI
        /// pueda resolver permisos, claims y etiquetas visibles.
        /// </summary>
        public async Task<Usuario?> ObtenerPorId(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Inserta un nuevo usuario en la base de datos.
        /// </summary>
        public async Task Crear(Usuario usuario)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Actualiza la entidad completa del usuario.
        /// Se usa, por ejemplo, para cambios de perfil, rol o imagen.
        /// </summary>
        public async Task Actualizar(Usuario usuario)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Entry(usuario).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Actualiza exclusivamente la contraseña persistida del usuario.
        /// </summary>
        public async Task ActualizarPassword(int usuarioId, string passwordHash)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var user = await context.Usuarios.FindAsync(usuarioId);
            if (user != null)
            {
                user.PasswordHash = passwordHash;
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Genera el hash SHA-256 utilizado actualmente por la aplicación
        /// para persistir y comparar contraseñas.
        /// </summary>
        public string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Verifica si el correo y la contraseña ingresados corresponden
        /// a un usuario registrado. Devuelve el usuario con su rol cargado.
        /// </summary>
        public async Task<Usuario?> VerificarCredenciales(string email, string password)
        {
            var passwordHash = HashPassword(password);
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => 
                    u.Email.ToLower() == email.Trim().ToLower() && 
                    u.PasswordHash == passwordHash);
        }
    }
}
