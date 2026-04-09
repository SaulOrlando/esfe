// RegistrationState.cs
// Propósito: Servicio de estado (scoped) usado durante el proceso de registro en varios pasos.
// Mantiene temporalmente los datos del formulario entre páginas del flujo de registro.
using Microsoft.AspNetCore.Components.Forms;

namespace ProyectoSocioEconomico.WebUI.Services;

public class RegistrationState
{
    // Progress Control
    public bool Step1Completed { get; set; } = false;
    public bool Step2Completed { get; set; } = false;

    // Step 1 Data
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string SelectedCountry { get; set; } = "El Salvador";

    // Step 2 Data
    public string DuiNumber { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public byte[]? DuiFrontData { get; set; }
    public string? DuiFrontName { get; set; }
    public byte[]? DuiBackData { get; set; }
    public string? DuiBackName { get; set; }

    // Step 3 Data
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public void Reset()
    {
        Step1Completed = false;
        Step2Completed = false;
        FirstName = string.Empty;
        LastName = string.Empty;
        PhoneNumber = string.Empty;
        SelectedCountry = "El Salvador";
        DuiNumber = string.Empty;
        BirthDate = null;
        DuiFrontData = null;
        DuiFrontName = null;
        DuiBackData = null;
        DuiBackName = null;
        Email = string.Empty;
        Password = string.Empty;
    }
}
