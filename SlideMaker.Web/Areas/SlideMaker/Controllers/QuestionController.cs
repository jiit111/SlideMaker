using Microsoft.AspNetCore.Mvc;
using SlideMaker.Web.Areas.SlideMaker.ViewModels;
using SlideMaker.Web.Data;
using SlideMaker.Web.Data.Seed;
using SlideMaker.Web.DTOs;
using SlideMaker.Web.Models;
using SlideMaker.Web.Services.AI;
using SlideMaker.Web.Services.Prompts;
using SlideMaker.Web.Services.Questions;
using Microsoft.EntityFrameworkCore;

namespace SlideMaker.Web.Areas.SlideMaker.Controllers;

[Area("SlideMaker")]
public class QuestionController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly ApplicationDbContext _db;
    private readonly IAIService _aiService;
    private readonly IPromptService _promptService;
    private readonly ILogger<QuestionController> _logger;

    public QuestionController(IQuestionService questionService, ApplicationDbContext db, IAIService aiService,
        IPromptService promptService, ILogger<QuestionController> logger)
    {
        _questionService = questionService;
        _db = db;
        _aiService = aiService;
        _promptService = promptService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Review(int presentationId, string ids, CancellationToken cancellationToken)
    {
        var questionIds = ParseIds(ids);
        var questions = await _questionService.GetByIdsAsync(questionIds, cancellationToken);
        var ordered = questionIds.Select(id => questions.FirstOrDefault(q => q.Id == id)).Where(q => q is not null).Cast<Question>();

        var viewModel = new QuestionReviewViewModel
        {
            PresentationId = presentationId,
            Questions = ordered.Select(ToCardViewModel).ToList()
        };
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> BankPicker(int presentationId, string? term, int? subjectId, CancellationToken cancellationToken)
    {
        var results = await _questionService.SearchAsync(term, subjectId, null, cancellationToken);
        var subjects = await _db.Subjects.OrderBy(s => s.Name).ToListAsync(cancellationToken);

        var viewModel = new QuestionBankPickerViewModel
        {
            PresentationId = presentationId,
            SearchTerm = term,
            SubjectId = subjectId,
            Subjects = subjects.Select(s => new SubjectOptionViewModel { Id = s.Id, Name = s.Name }).ToList(),
            Results = results.Select(ToCardViewModel).ToList()
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(QuestionCardViewModel model, CancellationToken cancellationToken)
    {
        var question = new Question
        {
            Id = model.Id,
            QuestionText = model.QuestionText,
            CorrectAnswer = model.CorrectAnswer,
            Explanation = model.Explanation,
            Difficulty = model.Difficulty,
            Language = model.Language,
            QuestionType = QuestionType.MultipleChoice,
            SortOrder = model.SortOrder,
            Options = model.Options.Select((o, i) => new QuestionOption
            {
                OptionLabel = o.Label,
                OptionText = o.Text,
                SortOrder = i,
                IsCorrect = string.Equals(o.Label, model.CorrectAnswer, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(o.Text, model.CorrectAnswer, StringComparison.OrdinalIgnoreCase)
            }).ToList()
        };

        await _questionService.UpdateAsync(question, cancellationToken);
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _questionService.DeleteAsync(id, cancellationToken);
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duplicate(int id, CancellationToken cancellationToken)
    {
        var source = await _questionService.GetAsync(id, cancellationToken);
        if (source is null)
        {
            return NotFound();
        }

        var copy = CloneQuestion(source);
        await _questionService.AddAsync(copy, cancellationToken);
        return Json(new { id = copy.Id, card = ToCardViewModel(copy) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Split(int id, CancellationToken cancellationToken)
    {
        var source = await _questionService.GetAsync(id, cancellationToken);
        if (source is null)
        {
            return NotFound();
        }

        var secondPart = CloneQuestion(source);
        secondPart.QuestionText = string.Empty;
        secondPart.CorrectAnswer = null;
        secondPart.Explanation = null;
        foreach (var opt in secondPart.Options)
        {
            opt.OptionText = string.Empty;
        }

        await _questionService.AddAsync(secondPart, cancellationToken);
        return Json(new { id = secondPart.Id, card = ToCardViewModel(secondPart) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Merge(int id, int nextId, CancellationToken cancellationToken)
    {
        var current = await _questionService.GetAsync(id, cancellationToken);
        var next = await _questionService.GetAsync(nextId, cancellationToken);
        if (current is null || next is null)
        {
            return NotFound();
        }

        // Build a detached update payload (see Regenerate for why reusing the tracked `current`
        // entity's Options collection directly would corrupt the change tracker).
        var merged = CloneQuestion(current);
        merged.Id = current.Id;
        merged.QuestionText = $"{current.QuestionText} {next.QuestionText}".Trim();
        merged.Explanation = string.IsNullOrWhiteSpace(current.Explanation) ? next.Explanation : current.Explanation;

        var updated = await _questionService.UpdateAsync(merged, cancellationToken);
        await _questionService.DeleteAsync(nextId, cancellationToken);

        return Json(new { card = ToCardViewModel(updated) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(CancellationToken cancellationToken)
    {
        var question = new Question
        {
            QuestionText = string.Empty,
            QuestionType = QuestionType.MultipleChoice,
            SourceType = QuestionSourceType.ManualEntry,
            CreatedByUserId = DbSeeder.DefaultUserId,
            CreatedOn = DateTime.UtcNow,
            Options = new List<QuestionOption>
            {
                new() { OptionLabel = "A", OptionText = string.Empty, SortOrder = 0 },
                new() { OptionLabel = "B", OptionText = string.Empty, SortOrder = 1 },
                new() { OptionLabel = "C", OptionText = string.Empty, SortOrder = 2 },
                new() { OptionLabel = "D", OptionText = string.Empty, SortOrder = 3 }
            }
        };

        await _questionService.AddAsync(question, cancellationToken);
        return Json(new { id = question.Id, card = ToCardViewModel(question) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Regenerate(int id, CancellationToken cancellationToken)
    {
        var existing = await _questionService.GetAsync(id, cancellationToken);
        if (existing is null)
        {
            return NotFound();
        }

        var prompt = _promptService.BuildFreeformGenerationPrompt(
            $"Generate one replacement multiple-choice question similar in topic and difficulty ({existing.Difficulty}) to: {existing.QuestionText}");

        var response = await _aiService.GenerateContentAsync(new AIRequest { Prompt = prompt, MaxQuestions = 1 }, cancellationToken);
        var generated = response.Questions.FirstOrDefault();

        if (!response.Success || generated is null)
        {
            return BadRequest(new { message = response.ErrorMessage ?? "The AI provider could not regenerate this question." });
        }

        // Build a detached update payload rather than mutating the tracked `existing` entity in
        // place — QuestionService.UpdateAsync loads its own tracked copy from the same DbContext
        // and re-parents options onto it, which corrupts the change tracker if `existing` is reused.
        var update = new Question
        {
            Id = existing.Id,
            QuestionText = generated.Question,
            CorrectAnswer = generated.CorrectAnswer,
            Explanation = generated.Explanation,
            Difficulty = existing.Difficulty,
            Language = existing.Language,
            QuestionType = existing.QuestionType,
            SubjectId = existing.SubjectId,
            ChapterId = existing.ChapterId,
            TopicId = existing.TopicId,
            SortOrder = existing.SortOrder,
            Options = generated.Options.Select((o, i) => new QuestionOption
            {
                OptionLabel = o.Label,
                OptionText = o.Text,
                SortOrder = i,
                IsCorrect = string.Equals(o.Label, generated.CorrectAnswer, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(o.Text, generated.CorrectAnswer, StringComparison.OrdinalIgnoreCase)
            }).ToList()
        };

        var updated = await _questionService.UpdateAsync(update, cancellationToken);
        return Json(new { card = ToCardViewModel(updated) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder([FromBody] List<int> orderedIds, CancellationToken cancellationToken)
    {
        var questions = await _questionService.GetByIdsAsync(orderedIds, cancellationToken);
        for (var i = 0; i < orderedIds.Count; i++)
        {
            var q = questions.FirstOrDefault(x => x.Id == orderedIds[i]);
            if (q is not null)
            {
                q.SortOrder = i + 1;
            }
        }
        await _db.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    private static List<int> ParseIds(string csv) =>
        (csv ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var v) ? v : (int?)null)
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

    private static Question CloneQuestion(Question source) => new()
    {
        QuestionText = source.QuestionText,
        CorrectAnswer = source.CorrectAnswer,
        Explanation = source.Explanation,
        Difficulty = source.Difficulty,
        Language = source.Language,
        QuestionType = source.QuestionType,
        SourceType = source.SourceType,
        SubjectId = source.SubjectId,
        ChapterId = source.ChapterId,
        TopicId = source.TopicId,
        SortOrder = source.SortOrder,
        CreatedByUserId = source.CreatedByUserId,
        CreatedOn = DateTime.UtcNow,
        Options = source.Options.OrderBy(o => o.SortOrder).Select(o => new QuestionOption
        {
            OptionLabel = o.OptionLabel,
            OptionText = o.OptionText,
            SortOrder = o.SortOrder,
            IsCorrect = o.IsCorrect
        }).ToList()
    };

    private static QuestionCardViewModel ToCardViewModel(Question q) => new()
    {
        Id = q.Id,
        SortOrder = q.SortOrder,
        QuestionText = q.QuestionText,
        CorrectAnswer = q.CorrectAnswer,
        Explanation = q.Explanation,
        Difficulty = q.Difficulty,
        Language = q.Language,
        Options = q.Options.OrderBy(o => o.SortOrder)
            .Select(o => new QuestionOptionViewModel { Label = o.OptionLabel, Text = o.OptionText }).ToList()
    };
}
