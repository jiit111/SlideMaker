using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SlideMaker.Web.Areas.SlideMaker.ViewModels;
using SlideMaker.Web.Data.Seed;
using SlideMaker.Web.DTOs;
using SlideMaker.Web.Models;
using SlideMaker.Web.Options;
using SlideMaker.Web.Services.Export;
using SlideMaker.Web.Services.Presentations;
using SlideMaker.Web.Services.SpecialDay;
using SlideMaker.Web.Services.Templates;

namespace SlideMaker.Web.Areas.SlideMaker.Controllers;

[Area("SlideMaker")]
public class PresentationController : Controller
{
    private readonly IPresentationService _presentationService;
    private readonly ITemplateService _templateService;
    private readonly IExportService _exportService;
    private readonly IPptxExportService _pptxExportService;
    private readonly ISpecialDayService _specialDayService;
    private readonly BrandingOptions _brandingOptions;

    public PresentationController(IPresentationService presentationService, ITemplateService templateService,
        IExportService exportService, IPptxExportService pptxExportService, ISpecialDayService specialDayService,
        IOptions<BrandingOptions> brandingOptions)
    {
        _presentationService = presentationService;
        _templateService = templateService;
        _exportService = exportService;
        _pptxExportService = pptxExportService;
        _specialDayService = specialDayService;
        _brandingOptions = brandingOptions.Value;
    }

    [HttpGet]
    public IActionResult Create(string? mode) =>
        View(new CreatePresentationViewModel { SourceMode = string.IsNullOrWhiteSpace(mode) ? "upload" : mode });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePresentationViewModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Please enter a presentation name.");
            return View(model);
        }

        var presentation = await _presentationService.CreateAsync(model.Name, model.Description, DbSeeder.DefaultUserId, cancellationToken);

        return model.SourceMode switch
        {
            "ai" => RedirectToAction("Generate", "AI", new { area = "SlideMaker", presentationId = presentation.Id }),
            "bank" => RedirectToAction("BankPicker", "Question", new { area = "SlideMaker", presentationId = presentation.Id }),
            _ => RedirectToAction("Image", "Upload", new { area = "SlideMaker", presentationId = presentation.Id })
        };
    }

    [HttpGet]
    public async Task<IActionResult> Pattern(int presentationId, string ids, CancellationToken cancellationToken)
    {
        var templates = await _templateService.GetAllAsync(cancellationToken);
        var defaultTemplate = templates.FirstOrDefault(t => t.IsDefault) ?? templates.FirstOrDefault();
        var existing = presentationId > 0 ? await _presentationService.GetAsync(presentationId, cancellationToken) : null;

        var eventDate = existing?.EventDate ?? DateTime.Today;

        var viewModel = new ChoosePatternViewModel
        {
            PresentationId = presentationId,
            QuestionIdsCsv = ids,
            QuestionCount = ParseIds(ids).Count,
            TemplateId = existing?.TemplateId ?? defaultTemplate?.Id ?? 0,
            Pattern = existing?.SlidePattern ?? SlidePattern.OneQuestionPerSlide,
            AvailableTemplates = templates.Select(t => new TemplateOptionViewModel
            {
                Id = t.Id,
                Name = t.Name,
                IsDefault = t.IsDefault
            }).ToList(),
            AcademyName = _brandingOptions.AcademyName,
            ClassName = existing?.ClassName,
            EventDate = eventDate,
            FestivalName = existing?.FestivalName ?? _specialDayService.GetSpecialDayName(eventDate),
            HeaderMessage = existing?.HeaderMessage,
            InstructorCredit = existing?.InstructorCredit
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pattern(ChoosePatternViewModel model, CancellationToken cancellationToken)
    {
        var ids = ParseIds(model.QuestionIdsCsv);
        if (ids.Count == 0 || model.TemplateId == 0)
        {
            TempData["Error"] = "Select at least one question and a template before generating slides.";
            return RedirectToAction("Pattern", new { presentationId = model.PresentationId, ids = model.QuestionIdsCsv });
        }

        var branding = new PresentationBrandingDto
        {
            AcademyName = string.IsNullOrWhiteSpace(model.AcademyName) ? _brandingOptions.AcademyName : model.AcademyName,
            ClassName = model.ClassName,
            EventDate = model.EventDate,
            FestivalName = model.FestivalName,
            HeaderMessage = model.HeaderMessage,
            InstructorCredit = model.InstructorCredit
        };

        await _presentationService.GenerateSlidesAsync(model.PresentationId, ids, model.TemplateId, model.Pattern, branding, cancellationToken);
        return RedirectToAction("Preview", new { id = model.PresentationId });
    }

    /// <summary>AJAX lookup used by the Pattern page's date picker to auto-suggest a festival name.</summary>
    [HttpGet]
    public IActionResult SpecialDay(DateTime date) => Json(new { name = _specialDayService.GetSpecialDayName(date) });

    [HttpGet]
    public async Task<IActionResult> Preview(int id, CancellationToken cancellationToken)
    {
        var presentation = await _presentationService.GetAsync(id, cancellationToken);
        if (presentation is null)
        {
            return NotFound();
        }

        var viewModel = ToPreviewViewModel(presentation);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, CancellationToken cancellationToken)
    {
        await _presentationService.MarkPublishedAsync(id, cancellationToken);
        TempData["Success"] = "Presentation saved.";
        return RedirectToAction("Index", "Home", new { area = "" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _presentationService.DeleteAsync(id, cancellationToken);
        return RedirectToAction("Index", "Home", new { area = "" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duplicate(int id, CancellationToken cancellationToken)
    {
        var copy = await _presentationService.DuplicateAsync(id, cancellationToken);
        return RedirectToAction("Preview", new { id = copy.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSlide(int presentationId, int slideId, CancellationToken cancellationToken)
    {
        await _presentationService.DeleteSlideAsync(presentationId, slideId, cancellationToken);
        return RedirectToAction("Preview", new { id = presentationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DuplicateSlide(int presentationId, int slideId, CancellationToken cancellationToken)
    {
        await _presentationService.DuplicateSlideAsync(presentationId, slideId, cancellationToken);
        return RedirectToAction("Preview", new { id = presentationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveSlide(int presentationId, int slideId, bool up, CancellationToken cancellationToken)
    {
        await _presentationService.MoveSlideAsync(presentationId, slideId, up, cancellationToken);
        return RedirectToAction("Preview", new { id = presentationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSlideElement(int presentationId, int elementId, string content, CancellationToken cancellationToken)
    {
        var updated = await _presentationService.UpdateSlideElementContentAsync(presentationId, elementId, content, cancellationToken);
        return updated ? Ok() : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> Export(int id, CancellationToken cancellationToken)
    {
        var presentation = await _presentationService.GetAsync(id, cancellationToken);
        if (presentation is null)
        {
            return NotFound();
        }

        var result = await _exportService.ExportToHtmlAsync(presentation, cancellationToken);
        if (!result.Success || result.FileBytes is null)
        {
            TempData["Error"] = result.ErrorMessage ?? "Export failed.";
            return RedirectToAction("Preview", new { id });
        }

        return File(result.FileBytes, result.ContentType ?? "application/octet-stream", result.FileName);
    }

    [HttpGet]
    public async Task<IActionResult> ExportPptx(int id, CancellationToken cancellationToken)
    {
        var presentation = await _presentationService.GetAsync(id, cancellationToken);
        if (presentation is null)
        {
            return NotFound();
        }

        var result = await _pptxExportService.ExportToPptxAsync(presentation, cancellationToken);
        if (!result.Success || result.FileBytes is null)
        {
            TempData["Error"] = result.ErrorMessage ?? "PPTX export failed.";
            return RedirectToAction("Preview", new { id });
        }

        return File(result.FileBytes, result.ContentType ?? "application/octet-stream", result.FileName);
    }

    private static List<int> ParseIds(string csv) =>
        csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var v) ? v : (int?)null)
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

    private static PresentationPreviewViewModel ToPreviewViewModel(Presentation presentation) => new()
    {
        Id = presentation.Id,
        Name = presentation.Name,
        Status = presentation.Status,
        TemplateName = presentation.Template?.Name,
        SlidePattern = presentation.SlidePattern,
        Slides = presentation.Slides.OrderBy(s => s.SortOrder).Select(s => new SlideViewModel
        {
            Id = s.Id,
            SlideNumber = s.SlideNumber,
            SlideType = s.SlideType,
            Title = s.Title,
            Background = s.Background,
            Elements = s.Elements.OrderBy(e => e.SortOrder).Select(e => new SlideElementViewModel
            {
                Id = e.Id,
                ElementType = e.ElementType,
                Content = e.Content,
                PositionX = e.PositionX,
                PositionY = e.PositionY,
                Width = e.Width,
                Height = e.Height,
                FontSize = e.FontSize,
                FontFamily = e.FontFamily,
                Alignment = e.Alignment,
                TextColor = e.TextColor,
                Background = e.Background,
                BorderRadius = e.BorderRadius,
                Padding = e.Padding,
                Bold = e.Bold
            }).ToList()
        }).ToList()
    };
}
