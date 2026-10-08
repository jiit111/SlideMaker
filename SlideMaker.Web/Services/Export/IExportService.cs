using SlideMaker.Web.Models;

namespace SlideMaker.Web.Services.Export;

public class ExportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public byte[]? FileBytes { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
}

/// <summary>
/// Export is provider-abstracted so PPTX/PDF/image exporters (Phase 2/3) can be added
/// as IPresentationExportService / IPdfExportService / IImageExportService without UI changes.
/// </summary>
public interface IExportService
{
    Task<ExportResult> ExportToHtmlAsync(Presentation presentation, CancellationToken cancellationToken = default);
}
