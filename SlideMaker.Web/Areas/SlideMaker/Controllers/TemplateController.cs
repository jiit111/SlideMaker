using Microsoft.AspNetCore.Mvc;
using SlideMaker.Web.Areas.SlideMaker.ViewModels;
using SlideMaker.Web.Data.Seed;
using SlideMaker.Web.Models;
using SlideMaker.Web.Services.Storage;
using SlideMaker.Web.Services.Templates;

namespace SlideMaker.Web.Areas.SlideMaker.Controllers;

[Area("SlideMaker")]
public class TemplateController : Controller
{
    private const string DefaultLayoutJson = """
    {
        "background": "linear-gradient(135deg, #0f0c29, #302b63, #24243e)",
        "headerColor": "#00e5a0",
        "accentColor": "#00e5a0",
        "fontFamily": "Segoe UI",
        "titleFontSize": 24,
        "bodyFontSize": 26,
        "textColor": "#f5f5f7",
        "questionBackground": "rgba(255,255,255,0.08)",
        "optionsBackground": "rgba(255,255,255,0.06)",
        "answerBackground": "rgba(0,229,160,0.16)",
        "answerTextColor": "#00e5a0",
        "explanationBackground": "rgba(255,255,255,0.05)",
        "borderRadius": 18,
        "cardPadding": 28,
        "questionPosition": { "x": 6, "y": 12, "width": 88, "height": 22 },
        "optionsPosition": { "x": 8, "y": 36, "width": 84, "height": 54 },
        "answerPosition": { "x": 6, "y": 20, "width": 88, "height": 22 },
        "explanationPosition": { "x": 6, "y": 46, "width": 88, "height": 42 }
    }
    """;

    private readonly ITemplateService _templateService;
    private readonly IFileStorageService _fileStorageService;

    public TemplateController(ITemplateService templateService, IFileStorageService fileStorageService)
    {
        _templateService = templateService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var templates = await _templateService.GetAllAsync(cancellationToken);
        var viewModel = new TemplateListViewModel
        {
            Templates = templates.Select(t => new TemplateCardViewModel
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                PreviewImagePath = t.PreviewImagePath,
                IsDefault = t.IsDefault,
                CreatedOn = t.CreatedOn,
                ModifiedOn = t.ModifiedOn
            }).ToList()
        };
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create() => View("Edit", new TemplateFormViewModel { LayoutJson = DefaultLayoutJson });

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var template = await _templateService.GetAsync(id, cancellationToken);
        if (template is null)
        {
            return NotFound();
        }

        return View(new TemplateFormViewModel
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            LayoutJson = template.LayoutJson
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(TemplateFormViewModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Please enter a template name.");
            return View("Edit", model);
        }

        if (model.Id == 0)
        {
            await _templateService.CreateAsync(new Template
            {
                Name = model.Name,
                Description = model.Description,
                LayoutJson = string.IsNullOrWhiteSpace(model.LayoutJson) ? DefaultLayoutJson : model.LayoutJson,
                CreatedByUserId = DbSeeder.DefaultUserId
            }, cancellationToken);
        }
        else
        {
            await _templateService.UpdateAsync(new Template
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                LayoutJson = model.LayoutJson
            }, cancellationToken);
        }

        TempData["Success"] = "Template saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Upload() => View(new TemplateUploadViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> Upload(TemplateUploadViewModel model, IFormFile? previewImage,
        IFormFile? templateFile, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            model.ErrorMessage = "Please enter a template name.";
            return View(model);
        }

        if ((previewImage is null || previewImage.Length == 0) && (templateFile is null || templateFile.Length == 0))
        {
            model.ErrorMessage = "Please choose a preview image and/or a template file to upload.";
            return View(model);
        }

        string? previewImageUrl = null;
        string? sourceFilePath = null;

        if (previewImage is { Length: > 0 })
        {
            await using var stream = previewImage.OpenReadStream();
            var saveResult = await _fileStorageService.SaveAsync(stream, previewImage.FileName, previewImage.ContentType,
                UploadPurpose.TemplatePreview, cancellationToken);
            if (!saveResult.Success)
            {
                model.ErrorMessage = saveResult.ErrorMessage;
                return View(model);
            }
            previewImageUrl = saveResult.RelativeUrl;
        }

        if (templateFile is { Length: > 0 })
        {
            await using var stream = templateFile.OpenReadStream();
            var saveResult = await _fileStorageService.SaveAsync(stream, templateFile.FileName, templateFile.ContentType,
                UploadPurpose.TemplateFile, cancellationToken);
            if (!saveResult.Success)
            {
                model.ErrorMessage = saveResult.ErrorMessage;
                return View(model);
            }
            sourceFilePath = saveResult.StoragePath;
        }

        // PPTX parsing isn't implemented yet (Phase 2) — the uploaded file is kept as SourceFilePath
        // for that later, while the template is immediately usable now with the default layout.
        await _templateService.CreateAsync(new Template
        {
            Name = model.Name,
            Description = model.Description,
            LayoutJson = DefaultLayoutJson,
            PreviewImagePath = previewImageUrl,
            SourceFilePath = sourceFilePath,
            CreatedByUserId = DbSeeder.DefaultUserId
        }, cancellationToken);

        TempData["Success"] = "Template uploaded.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duplicate(int id, CancellationToken cancellationToken)
    {
        await _templateService.DuplicateAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _templateService.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefault(int id, CancellationToken cancellationToken)
    {
        await _templateService.SetDefaultAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
