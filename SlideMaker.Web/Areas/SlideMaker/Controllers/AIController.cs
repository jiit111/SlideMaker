using Microsoft.AspNetCore.Mvc;
using SlideMaker.Web.Areas.SlideMaker.ViewModels;
using SlideMaker.Web.Data;
using SlideMaker.Web.Data.Seed;
using SlideMaker.Web.DTOs;
using SlideMaker.Web.Models;
using SlideMaker.Web.Services.AI;
using SlideMaker.Web.Services.Presentations;
using SlideMaker.Web.Services.Prompts;
using SlideMaker.Web.Services.Questions;
using Microsoft.EntityFrameworkCore;

namespace SlideMaker.Web.Areas.SlideMaker.Controllers;

[Area("SlideMaker")]
public class AIController : Controller
{
    private readonly IAIService _aiService;
    private readonly IPromptService _promptService;
    private readonly IQuestionService _questionService;
    private readonly IPresentationService _presentationService;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AIController> _logger;

    public AIController(IAIService aiService, IPromptService promptService, IQuestionService questionService,
        IPresentationService presentationService, ApplicationDbContext db, ILogger<AIController> logger)
    {
        _aiService = aiService;
        _promptService = promptService;
        _questionService = questionService;
        _presentationService = presentationService;
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Generate(int presentationId) => View(new AIGenerateViewModel { PresentationId = presentationId });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(AIGenerateViewModel model, CancellationToken cancellationToken)
    {
        var request = new AIQuestionGenerationRequest
        {
            Subject = model.Subject,
            ClassLevel = model.ClassLevel,
            Chapter = model.Chapter,
            Topic = model.Topic,
            NumberOfQuestions = model.NumberOfQuestions,
            QuestionType = model.QuestionType,
            Difficulty = model.Difficulty,
            Language = model.Language,
            AdditionalInstruction = model.AdditionalInstruction
        };

        var prompt = _promptService.BuildQuestionGenerationPrompt(request);
        var aiRequest = new AIRequest { Prompt = prompt, MaxQuestions = model.NumberOfQuestions };

        var response = await _aiService.GenerateContentAsync(aiRequest, cancellationToken);
        var validation = AIQuestionValidator.Validate(response.Questions, model.NumberOfQuestions);

        await LogRequestAsync("QuestionGeneration", response, validation, model.NumberOfQuestions, cancellationToken);

        if (!response.Success)
        {
            model.ValidationErrors = new List<string> { response.ErrorMessage ?? "The AI provider did not return a response." };
            return View(model);
        }

        if (!validation.IsValid)
        {
            model.ValidationErrors = validation.Errors;
            return View(model);
        }

        var ids = await SaveGeneratedQuestionsAsync(response.Questions, cancellationToken);
        return RedirectToAction("Review", "Question", new { presentationId = model.PresentationId, ids = string.Join(',', ids) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickGenerate(QuickAIViewModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.RequirementText))
        {
            TempData["Error"] = "Please describe what you'd like to create.";
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        var presentationName = model.RequirementText.Length > 80
            ? model.RequirementText[..80] + "…"
            : model.RequirementText;
        var presentation = await _presentationService.CreateAsync(presentationName, model.RequirementText, DbSeeder.DefaultUserId, cancellationToken);

        var prompt = _promptService.BuildFreeformGenerationPrompt(model.RequirementText);
        var response = await _aiService.GenerateContentAsync(new AIRequest { Prompt = prompt }, cancellationToken);
        var validation = AIQuestionValidator.Validate(response.Questions, null);

        await LogRequestAsync("FreeformGeneration", response, validation, null, cancellationToken);

        if (!response.Success || !validation.IsValid)
        {
            TempData["Error"] = response.ErrorMessage ?? string.Join(" ", validation.Errors);
            return RedirectToAction("Generate", new { presentationId = presentation.Id });
        }

        var ids = await SaveGeneratedQuestionsAsync(response.Questions, cancellationToken);
        return RedirectToAction("Review", "Question", new { presentationId = presentation.Id, ids = string.Join(',', ids) });
    }

    private async Task<List<int>> SaveGeneratedQuestionsAsync(List<AIGeneratedQuestionDto> generated, CancellationToken cancellationToken)
    {
        var ids = new List<int>();
        foreach (var dto in generated)
        {
            var question = _questionService.FromAIGenerated(dto, DbSeeder.DefaultUserId);
            await _questionService.AddAsync(question, cancellationToken);
            ids.Add(question.Id);
        }
        return ids;
    }

    private async Task LogRequestAsync(string requestType, AIResponse response, AIValidationResult validation,
        int? requestedCount, CancellationToken cancellationToken)
    {
        _db.AIRequestLogs.Add(new AIRequestLog
        {
            Provider = _aiService.ProviderName,
            RequestType = requestType,
            Success = response.Success && validation.IsValid,
            ErrorMessage = response.Success ? (validation.IsValid ? null : string.Join(" ", validation.Errors)) : response.ErrorMessage,
            QuestionCountRequested = requestedCount,
            QuestionCountReturned = response.Questions.Count,
            CreatedByUserId = DbSeeder.DefaultUserId,
            CreatedOn = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
    }
}
