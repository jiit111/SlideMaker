using SlideMaker.Web.DTOs;
using SlideMaker.Web.Models;

namespace SlideMaker.Web.Services.Presentations;

public interface IPresentationService
{
    Task<Presentation> CreateAsync(string name, string? description, int createdByUserId, CancellationToken cancellationToken = default);
    Task<Presentation?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Presentation>> GetRecentAsync(int take = 10, CancellationToken cancellationToken = default);
    Task<Presentation> GenerateSlidesAsync(int presentationId, List<int> questionIds, int templateId, SlidePattern pattern,
        PresentationBrandingDto? branding = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Presentation> DuplicateAsync(int id, CancellationToken cancellationToken = default);
    Task MarkPublishedAsync(int id, CancellationToken cancellationToken = default);

    Task DeleteSlideAsync(int presentationId, int slideId, CancellationToken cancellationToken = default);
    Task DuplicateSlideAsync(int presentationId, int slideId, CancellationToken cancellationToken = default);
    Task MoveSlideAsync(int presentationId, int slideId, bool moveUp, CancellationToken cancellationToken = default);

    /// <summary>Direct "fix what's on the slide" edit — updates one element's text in place,
    /// without touching the underlying Question record. Returns false if the element isn't
    /// found under the given presentation.</summary>
    Task<bool> UpdateSlideElementContentAsync(int presentationId, int elementId, string content, CancellationToken cancellationToken = default);
}
