// NewCaseState.cs
// Propósito: Servicio de estado (scoped) que mantiene datos temporales durante el flujo de
// creación de un nuevo caso en varios pasos (Step1/Step2). Se registra como scoped en Program.cs.
using System.ComponentModel.DataAnnotations;

namespace ProyectoSocioEconomico.WebUI.Services;

/// <summary>
/// Estado temporal del wizard de creación de casos.
/// Conserva los datos entre pasos antes de construir la entidad final.
/// </summary>
public class NewCaseState
{
    // Progress Control
    public bool Step1Completed { get; set; } = false;
    public bool Step2Completed { get; set; } = false;

    // Step 1 Data
    [Required(ErrorMessage = "Please select a category")]
    public int? CategoryId { get; set; }

    [Required(ErrorMessage = "The campaign title is required")]
    [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
    public string CaseTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [MinLength(10, ErrorMessage = "Please provide a more detailed description (at least 10 characters)")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Financial goal is required")]
    [Range(1, 1000000, ErrorMessage = "Goal must be between $1 and $1,000,000")]
    public decimal FinancialGoal { get; set; }

    // Step 2 Data
    public List<byte[]> EvidenceFiles { get; set; } = new();
    public List<string> EvidenceFileNames { get; set; } = new();
    
    public byte[]? ThumbnailData { get; set; }
    public string? ThumbnailName { get; set; }

    public string SelectedMethod { get; set; } = "bank";
    public string AccountHolder { get; set; } = string.Empty;

    public string AccountNumber { get; set; } = string.Empty;

    public string BankName { get; set; } = string.Empty;

    public bool IsCertified { get; set; } = false;

    /// <summary>
    /// Limpia completamente el estado del formulario una vez que
    /// el caso fue creado o cuando el flujo debe reiniciarse.
    /// </summary>
    public void Reset()
    {
        Step1Completed = false;
        Step2Completed = false;
        CategoryId = null;
        CaseTitle = string.Empty;
        Description = string.Empty;
        FinancialGoal = 0;
        EvidenceFiles.Clear();
        EvidenceFileNames.Clear();
        ThumbnailData = null;
        ThumbnailName = null;
        SelectedMethod = "bank";
        AccountHolder = string.Empty;
        AccountNumber = string.Empty;
        BankName = string.Empty;
        IsCertified = false;
    }
}
