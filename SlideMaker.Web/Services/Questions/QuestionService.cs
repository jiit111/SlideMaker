using Microsoft.EntityFrameworkCore;
using SlideMaker.Web.Data;
using SlideMaker.Web.DTOs;
using SlideMaker.Web.Models;

namespace SlideMaker.Web.Services.Questions;

public class QuestionService : IQuestionService
{
    private readonly ApplicationDbContext _db;

    public QuestionService(ApplicationDbContext db)
    {
        _db = db;
    }

    public Question FromExtracted(ExtractedQuestionDto dto, int createdByUserId)
    {
        var question = new Question
        {
            QuestionText = dto.QuestionText,
            CorrectAnswer = dto.Answer,
            Explanation = dto.Explanation,
            QuestionType = QuestionType.MultipleChoice,
            SourceType = QuestionSourceType.ImageOcr,
            SortOrder = dto.QuestionNumber,
            CreatedByUserId = createdByUserId,
            CreatedOn = DateTime.UtcNow
        };

        for (var i = 0; i < dto.Options.Count; i++)
        {
            question.Options.Add(new QuestionOption
            {
                OptionLabel = dto.Options[i].Label,
                OptionText = dto.Options[i].Text,
                SortOrder = i,
                IsCorrect = !string.IsNullOrWhiteSpace(dto.Answer) &&
                            (string.Equals(dto.Options[i].Label, dto.Answer, StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(dto.Options[i].Text, dto.Answer, StringComparison.OrdinalIgnoreCase))
            });
        }

        return question;
    }

    public Question FromAIGenerated(AIGeneratedQuestionDto dto, int createdByUserId)
    {
        var difficulty = Enum.TryParse<DifficultyLevel>(dto.Difficulty, true, out var parsedDifficulty)
            ? parsedDifficulty
            : DifficultyLevel.Medium;

        var question = new Question
        {
            QuestionText = dto.Question,
            CorrectAnswer = dto.CorrectAnswer,
            Explanation = dto.Explanation,
            Difficulty = difficulty,
            QuestionType = QuestionType.MultipleChoice,
            SourceType = QuestionSourceType.AIGenerated,
            CreatedByUserId = createdByUserId,
            CreatedOn = DateTime.UtcNow
        };

        for (var i = 0; i < dto.Options.Count; i++)
        {
            question.Options.Add(new QuestionOption
            {
                OptionLabel = dto.Options[i].Label,
                OptionText = dto.Options[i].Text,
                SortOrder = i,
                IsCorrect = string.Equals(dto.Options[i].Label, dto.CorrectAnswer, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(dto.Options[i].Text, dto.CorrectAnswer, StringComparison.OrdinalIgnoreCase)
            });
        }

        return question;
    }

    public async Task<Question> AddAsync(Question question, CancellationToken cancellationToken = default)
    {
        _db.Questions.Add(question);
        await _db.SaveChangesAsync(cancellationToken);
        return question;
    }

    public Task<Question?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        _db.Questions.Include(q => q.Options).FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public Task<List<Question>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default) =>
        _db.Questions.Include(q => q.Options).Where(q => ids.Contains(q.Id)).ToListAsync(cancellationToken);

    public async Task<Question> UpdateAsync(Question question, CancellationToken cancellationToken = default)
    {
        var existing = await _db.Questions.Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == question.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Question {question.Id} not found.");

        existing.QuestionText = question.QuestionText;
        existing.CorrectAnswer = question.CorrectAnswer;
        existing.Explanation = question.Explanation;
        existing.Difficulty = question.Difficulty;
        existing.Language = question.Language;
        existing.QuestionType = question.QuestionType;
        existing.SubjectId = question.SubjectId;
        existing.ChapterId = question.ChapterId;
        existing.TopicId = question.TopicId;
        existing.SortOrder = question.SortOrder;
        existing.ModifiedOn = DateTime.UtcNow;

        _db.QuestionOptions.RemoveRange(existing.Options);
        existing.Options = question.Options;

        await _db.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var question = await _db.Questions.FindAsync([id], cancellationToken);
        if (question is not null)
        {
            _db.Questions.Remove(question);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<Question>> SearchAsync(string? term, int? subjectId, int? chapterId, CancellationToken cancellationToken = default)
    {
        var query = _db.Questions.Include(q => q.Options).Include(q => q.Subject).Include(q => q.Chapter).AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(q => q.QuestionText.Contains(term));
        }
        if (subjectId.HasValue)
        {
            query = query.Where(q => q.SubjectId == subjectId.Value);
        }
        if (chapterId.HasValue)
        {
            query = query.Where(q => q.ChapterId == chapterId.Value);
        }

        return await query.OrderByDescending(q => q.CreatedOn).Take(200).ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        _db.Questions.CountAsync(cancellationToken);
}
