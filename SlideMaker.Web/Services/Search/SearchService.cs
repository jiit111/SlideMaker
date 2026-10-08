using Microsoft.EntityFrameworkCore;
using SlideMaker.Web.Data;

namespace SlideMaker.Web.Services.Search;

public class SearchService : ISearchService
{
    private readonly ApplicationDbContext _db;

    public SearchService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<SearchResults> SearchAsync(string term, CancellationToken cancellationToken = default)
    {
        var results = new SearchResults();
        if (string.IsNullOrWhiteSpace(term))
        {
            return results;
        }

        var questions = await _db.Questions
            .Where(q => q.QuestionText.Contains(term))
            .OrderByDescending(q => q.CreatedOn)
            .Take(15)
            .ToListAsync(cancellationToken);
        results.Questions = questions.Select(q => new SearchResultItem
        {
            Title = q.QuestionText.Length > 80 ? q.QuestionText[..80] + "…" : q.QuestionText,
            Subtitle = q.Difficulty.ToString(),
            Url = $"/SlideMaker/Question/Review?presentationId=0&ids={q.Id}"
        }).ToList();

        var templates = await _db.Templates
            .Where(t => t.IsActive && t.Name.Contains(term))
            .Take(15)
            .ToListAsync(cancellationToken);
        results.Templates = templates.Select(t => new SearchResultItem
        {
            Title = t.Name,
            Subtitle = t.Description,
            Url = $"/SlideMaker/Template/Edit/{t.Id}"
        }).ToList();

        var presentations = await _db.Presentations
            .Where(p => p.Name.Contains(term))
            .OrderByDescending(p => p.CreatedOn)
            .Take(15)
            .ToListAsync(cancellationToken);
        results.Presentations = presentations.Select(p => new SearchResultItem
        {
            Title = p.Name,
            Subtitle = p.Status.ToString(),
            Url = $"/SlideMaker/Presentation/Preview/{p.Id}"
        }).ToList();

        return results;
    }
}
