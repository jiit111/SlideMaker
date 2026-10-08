using Microsoft.AspNetCore.Mvc;
using SlideMaker.Web.Areas.SlideMaker.ViewModels;
using SlideMaker.Web.Data.Seed;
using SlideMaker.Web.Models;
using SlideMaker.Web.Services.OCR;
using SlideMaker.Web.Services.Questions;
using SlideMaker.Web.Services.Storage;

namespace SlideMaker.Web.Areas.SlideMaker.Controllers;

[Area("SlideMaker")]
public class UploadController : Controller
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IOcrService _ocrService;
    private readonly IQuestionExtractionService _extractionService;
    private readonly IQuestionService _questionService;
    private readonly ILogger<UploadController> _logger;

    public UploadController(IFileStorageService fileStorageService, IOcrService ocrService,
        IQuestionExtractionService extractionService, IQuestionService questionService,
        ILogger<UploadController> logger)
    {
        _fileStorageService = fileStorageService;
        _ocrService = ocrService;
        _extractionService = extractionService;
        _questionService = questionService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Image(int presentationId) => View(new UploadImageViewModel { PresentationId = presentationId });

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> Image(int presentationId, List<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files.Count == 0)
        {
            return View(new UploadImageViewModel { PresentationId = presentationId, ErrorMessage = "Please choose at least one image." });
        }

        var createdIds = new List<int>();
        var questionNumberOffset = 0;
        string? lastError = null;

        foreach (var file in files)
        {
            await using var stream = file.OpenReadStream();
            var saveResult = await _fileStorageService.SaveAsync(stream, file.FileName, file.ContentType, UploadPurpose.QuestionImage, cancellationToken);
            if (!saveResult.Success)
            {
                lastError = saveResult.ErrorMessage;
                continue;
            }

            await using var readStream = System.IO.File.OpenRead(saveResult.StoragePath!);
            var ocrResult = await _ocrService.ExtractTextAsync(readStream, cancellationToken);
            if (!ocrResult.Success)
            {
                lastError = ocrResult.ErrorMessage;
                continue;
            }

            var extracted = _extractionService.ParseQuestions(ocrResult.RawText);
            foreach (var dto in extracted)
            {
                dto.QuestionNumber += questionNumberOffset;
                var question = _questionService.FromExtracted(dto, DbSeeder.DefaultUserId);
                await _questionService.AddAsync(question, cancellationToken);
                createdIds.Add(question.Id);
            }
            questionNumberOffset += extracted.Count;
        }

        if (createdIds.Count == 0)
        {
            TempData["Error"] = lastError ?? "No questions could be detected in the uploaded image(s). Add questions manually below.";
        }

        return RedirectToAction("Review", "Question", new { presentationId, ids = string.Join(',', createdIds) });
    }
}
