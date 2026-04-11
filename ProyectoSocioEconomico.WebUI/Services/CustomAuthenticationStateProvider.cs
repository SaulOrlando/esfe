// CustomAuthenticationStateProvider.cs
// Propósito: AuthenticationStateProvider personalizado que gestiona la sesión de usuario
// almacenando/recuperando un objeto Usuario en ProtectedLocalStorage y emitiendo cambios
// de estado de autenticación a la aplicación Blazor.
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using ProyectoSocioEconomico.Application.Interfaces;
using ProyectoSocioEconomico.Domain.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace ProyectoSocioEconomico.WebUI.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedLocalStorage _localStorage;
        private readonly IUsuarioService _usuarioService;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(
            ProtectedLocalStorage localStorage,
            IUsuarioService usuarioService)
        {
            _localStorage = localStorage;
            _usuarioService = usuarioService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userSessionStorageResult = await _localStorage.GetAsync<string>("UserSession");
                var userSession = userSessionStorageResult.Success ? userSessionStorageResult.Value : null;

                if (string.IsNullOrWhiteSpace(userSession))
                    return new AuthenticationState(_anonymous);

                var usuario = JsonSerializer.Deserialize<Usuario>(userSession);
                if (usuario == null)
                    return new AuthenticationState(_anonymous);

                var usuarioActualizado = await _usuarioService.ObtenerPorId(usuario.Id);
                if (usuarioActualizado != null)
                {
                    usuario = usuarioActualizado;
                    await _localStorage.SetAsync("UserSession", JsonSerializer.Serialize(usuario));
                }

                var claimsPrincipal = CreateClaimsPrincipalFromUser(usuario);
                return new AuthenticationState(claimsPrincipal);
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public async Task NotifyUserLogin(Usuario usuario)
        {
            var userSession = JsonSerializer.Serialize(usuario);
            await _localStorage.SetAsync("UserSession", userSession);

            var claimsPrincipal = CreateClaimsPrincipalFromUser(usuario);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public async Task NotifyUserLogout()
        {
            await _localStorage.DeleteAsync("UserSession");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        private ClaimsPrincipal CreateClaimsPrincipalFromUser(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim("ImagenPerfil", usuario.ImagenPerfil ?? "uploads/profiles/DefaultProfile.png")
            };

            // Add role claim based on the navigation property if it's available.
            var roleName = usuario.IdRolNavigation?.Nombre ?? "Usuario";
            claims.Add(new Claim(ClaimTypes.Role, roleName));

            var identity = new ClaimsIdentity(claims, "CustomAuth");
            return new ClaimsPrincipal(identity);
        }
    }
}
