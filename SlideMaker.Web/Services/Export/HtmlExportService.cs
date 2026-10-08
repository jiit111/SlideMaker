using System.Net;
using System.Text;
using SlideMaker.Web.Models;

namespace SlideMaker.Web.Services.Export;

/// <summary>Basic printable/exportable HTML rendering. PPTX/PDF exporters are Phase 2/3.</summary>
public class HtmlExportService : IExportService
{
    public Task<ExportResult> ExportToHtmlAsync(Presentation presentation, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!doctype html><html><head><meta charset=\"utf-8\">");
        sb.AppendLine($"<title>{WebUtility.HtmlEncode(presentation.Name)}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body{font-family:Segoe UI,Arial,sans-serif;background:#eee;margin:0;padding:20px;}");
        sb.AppendLine(".slide{position:relative;width:960px;height:540px;margin:0 auto 30px auto;box-shadow:0 2px 8px rgba(0,0,0,.2);overflow:hidden;page-break-after:always;}");
        sb.AppendLine(".el{position:absolute;white-space:pre-wrap;}");
        sb.AppendLine("</style></head><body>");

        foreach (var slide in presentation.Slides.OrderBy(s => s.SortOrder))
        {
            sb.AppendLine($"<div class=\"slide\" style=\"background:{WebUtility.HtmlEncode(slide.Background ?? "#ffffff")}\">");
            foreach (var element in slide.Elements.OrderBy(e => e.SortOrder))
            {
                var style = SlideElementStyleHelper.BuildStyle(element.PositionX, element.PositionY, element.Width, element.Height,
                    element.FontSize, element.FontFamily, element.Alignment, element.TextColor, element.Background,
                    element.BorderRadius, element.Padding, element.Bold);
                sb.AppendLine($"<div class=\"el\" style=\"{style}\">{WebUtility.HtmlEncode(element.Content)}</div>");
            }
            sb.AppendLine("</div>");
        }

        sb.AppendLine("</body></html>");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return Task.FromResult(new ExportResult
        {
            Success = true,
            FileBytes = bytes,
            FileName = $"{SanitizeFileName(presentation.Name)}.html",
            ContentType = "text/html"
        });
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Where(c => !invalid.Contains(c)).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "presentation" : cleaned;
    }
}
