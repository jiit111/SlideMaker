using SlideMaker.Web.Models;

namespace SlideMaker.Web.Services.Export;

/// <summary>
/// Generates a real, editable .pptx file via the free/open-source DocumentFormat.OpenXml SDK
/// (no PowerPoint installation required to generate it). Kept as its own interface from
/// IExportService so future PDF/image exporters can be added the same way without bloating
/// one interface (spec: IPresentationExportService / IPdfExportService / IImageExportService).
/// </summary>
public interface IPptxExportService
{
    Task<ExportResult> ExportToPptxAsync(Presentation presentation, CancellationToken cancellationToken = default);
}
