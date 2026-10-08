using SlideMaker.Web.DTOs;
using SlideMaker.Web.Models;

namespace SlideMaker.Web.Services.Questions;

public interface IQuestionService
{
    Question FromExtracted(ExtractedQuestionDto dto, int createdByUserId);
    Question FromAIGenerated(AIGeneratedQuestionDto dto, int createdByUserId);

    Task<Question> AddAsync(Question question, CancellationToken cancellationToken = default);
    Task<Question?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Question>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    Task<Question> UpdateAsync(Question question, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Question>> SearchAsync(string? term, int? subjectId, int? chapterId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
