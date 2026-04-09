// ButtonViewModel.cs
// Propósito: Modelo de vista simple que representa la configuración de un botón (clase reutilizable
// en componentes de la UI). Incluye la clase y propiedades que describen la apariencia y destino.

namespace ProyectoSocioEconomico.WebUI.ViewModels
{
    public class ButtonViewModel
    {
        // Clase que contiene valores para construir botones desde plantillas/razor
        public string? ColorClass { get; set; }
        public string? Path { get; set; }
        public string? TextoBtn { get; set; }
    }
}
