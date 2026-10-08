using Microsoft.EntityFrameworkCore;
using SlideMaker.Web.Data;
using SlideMaker.Web.DTOs;
using SlideMaker.Web.Models;
using SlideMaker.Web.Services.Questions;
using SlideMaker.Web.Services.Slides;

namespace SlideMaker.Web.Services.Presentations;

public class PresentationService : IPresentationService
{
    private readonly ApplicationDbContext _db;
    private readonly IQuestionService _questionService;
    private readonly ISlideGenerationService _slideGenerationService;

    public PresentationService(ApplicationDbContext db, IQuestionService questionService,
        ISlideGenerationService slideGenerationService)
    {
        _db = db;
        _questionService = questionService;
        _slideGenerationService = slideGenerationService;
    }

    public async Task<Presentation> CreateAsync(string name, string? description, int createdByUserId, CancellationToken cancellationToken = default)
    {
        var presentation = new Presentation
        {
            Name = name,
            Description = description,
            Status = PresentationStatus.Draft,
            CreatedByUserId = createdByUserId,
            CreatedOn = DateTime.UtcNow
        };
        _db.Presentations.Add(presentation);
        await _db.SaveChangesAsync(cancellationToken);
        return presentation;
    }

    public Task<Presentation?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        _db.Presentations
            .Include(p => p.Template)
            .Include(p => p.Slides.OrderBy(s => s.SortOrder))
                .ThenInclude(s => s.Elements.OrderBy(e => e.SortOrder))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<List<Presentation>> GetRecentAsync(int take = 10, CancellationToken cancellationToken = default) =>
        _db.Presentations
            .Include(p => p.Template)
            .Include(p => p.Slides)
            .OrderByDescending(p => p.CreatedOn)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<Presentation> GenerateSlidesAsync(int presentationId, List<int> questionIds, int templateId,
        SlidePattern pattern, PresentationBrandingDto? branding = null, CancellationToken cancellationToken = default)
    {
        var presentation = await _db.Presentations.Include(p => p.Slides).ThenInclude(s => s.Elements)
            .FirstOrDefaultAsync(p => p.Id == presentationId, cancellationToken)
            ?? throw new InvalidOperationException($"Presentation {presentationId} not found.");

        var template = await _db.Templates.FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken)
            ?? throw new InvalidOperationException($"Template {templateId} not found.");

        var questions = await _questionService.GetByIdsAsync(questionIds, cancellationToken);
        var orderedQuestions = questionIds.Select(id => questions.First(q => q.Id == id)).ToList();

        if (presentation.Slides.Count > 0)
        {
            _db.Slides.RemoveRange(presentation.Slides);
        }

        var newSlides = _slideGenerationService.GenerateSlides(orderedQuestions, template, pattern, branding);
        foreach (var slide in newSlides)
        {
            slide.PresentationId = presentation.Id;
        }

        presentation.Slides = newSlides;
        presentation.TemplateId = template.Id;
        presentation.SlidePattern = pattern;
        presentation.Status = PresentationStatus.Generated;

        if (branding is not null)
        {
            presentation.ClassName = branding.ClassName;
            presentation.EventDate = branding.EventDate;
            presentation.FestivalName = branding.FestivalName;
            presentation.HeaderMessage = branding.HeaderMessage;
            presentation.InstructorCredit = branding.InstructorCredit;
        }
        presentation.ModifiedOn = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return presentation;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var presentation = await _db.Presentations.FindAsync([id], cancellationToken);
        if (presentation is not null)
        {
            _db.Presentations.Remove(presentation);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<Presentation> DuplicateAsync(int id, CancellationToken cancellationToken = default)
    {
        var source = await GetAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Presentation {id} not found.");

        var copy = new Presentation
        {
            Name = $"{source.Name} (Copy)",
            Description = source.Description,
            TemplateId = source.TemplateId,
            SlidePattern = source.SlidePattern,
            Status = PresentationStatus.Draft,
            CreatedByUserId = source.CreatedByUserId,
            CreatedOn = DateTime.UtcNow
        };

        foreach (var slide in source.Slides.OrderBy(s => s.SortOrder))
        {
            var slideCopy = new Slide
            {
                SlideNumber = slide.SlideNumber,
                SortOrder = slide.SortOrder,
                SlideType = slide.SlideType,
                Title = slide.Title,
                Content = slide.Content,
                Background = slide.Background,
                Layout = slide.Layout,
                QuestionId = slide.QuestionId
            };
            foreach (var element in slide.Elements.OrderBy(e => e.SortOrder))
            {
                slideCopy.Elements.Add(new SlideElement
                {
                    ElementType = element.ElementType,
                    Content = element.Content,
                    PositionX = element.PositionX,
                    PositionY = element.PositionY,
                    Width = element.Width,
                    Height = element.Height,
                    FontSize = element.FontSize,
                    FontFamily = element.FontFamily,
                    Alignment = element.Alignment,
                    SortOrder = element.SortOrder
                });
            }
            copy.Slides.Add(slideCopy);
        }

        _db.Presentations.Add(copy);
        await _db.SaveChangesAsync(cancellationToken);
        return copy;
    }

    public async Task MarkPublishedAsync(int id, CancellationToken cancellationToken = default)
    {
        var presentation = await _db.Presentations.FindAsync([id], cancellationToken);
        if (presentation is not null)
        {
            presentation.Status = PresentationStatus.Published;
            presentation.ModifiedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteSlideAsync(int presentationId, int slideId, CancellationToken cancellationToken = default)
    {
        var slide = await _db.Slides.FirstOrDefaultAsync(s => s.Id == slideId && s.PresentationId == presentationId, cancellationToken);
        if (slide is null)
        {
            return;
        }
        _db.Slides.Remove(slide);
        await _db.SaveChangesAsync(cancellationToken);
        await RenumberSlidesAsync(presentationId, cancellationToken);
    }

    public async Task DuplicateSlideAsync(int presentationId, int slideId, CancellationToken cancellationToken = default)
    {
        var slide = await _db.Slides.Include(s => s.Elements)
            .FirstOrDefaultAsync(s => s.Id == slideId && s.PresentationId == presentationId, cancellationToken);
        if (slide is null)
        {
            return;
        }

        var copy = new Slide
        {
            PresentationId = presentationId,
            SlideType = slide.SlideType,
            Title = slide.Title,
            Content = slide.Content,
            Background = slide.Background,
            Layout = slide.Layout,
            QuestionId = slide.QuestionId,
            SlideNumber = slide.SlideNumber + 1,
            SortOrder = slide.SortOrder + 1
        };
        foreach (var element in slide.Elements)
        {
            copy.Elements.Add(new SlideElement
            {
                ElementType = element.ElementType,
                Content = element.Content,
                PositionX = element.PositionX,
                PositionY = element.PositionY,
                Width = element.Width,
                Height = element.Height,
                FontSize = element.FontSize,
                FontFamily = element.FontFamily,
                Alignment = element.Alignment,
                SortOrder = element.SortOrder
            });
        }

        _db.Slides.Add(copy);
        await _db.SaveChangesAsync(cancellationToken);
        await RenumberSlidesAsync(presentationId, cancellationToken);
    }

    public async Task MoveSlideAsync(int presentationId, int slideId, bool moveUp, CancellationToken cancellationToken = default)
    {
        var slides = await _db.Slides.Where(s => s.PresentationId == presentationId)
            .OrderBy(s => s.SortOrder).ToListAsync(cancellationToken);

        var index = slides.FindIndex(s => s.Id == slideId);
        var targetIndex = moveUp ? index - 1 : index + 1;
        if (index < 0 || targetIndex < 0 || targetIndex >= slides.Count)
        {
            return;
        }

        (slides[index].SortOrder, slides[targetIndex].SortOrder) = (slides[targetIndex].SortOrder, slides[index].SortOrder);
        await _db.SaveChangesAsync(cancellationToken);
        await RenumberSlidesAsync(presentationId, cancellationToken);
    }

    public async Task<bool> UpdateSlideElementContentAsync(int presentationId, int elementId, string content, CancellationToken cancellationToken = default)
    {
        var element = await _db.SlideElements
            .Include(e => e.Slide)
            .FirstOrDefaultAsync(e => e.Id == elementId && e.Slide.PresentationId == presentationId, cancellationToken);

        if (element is null)
        {
            return false;
        }

        element.Content = content;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task RenumberSlidesAsync(int presentationId, CancellationToken cancellationToken)
    {
        var slides = await _db.Slides.Where(s => s.PresentationId == presentationId)
            .OrderBy(s => s.SortOrder).ToListAsync(cancellationToken);

        for (var i = 0; i < slides.Count; i++)
        {
            slides[i].SlideNumber = i + 1;
            slides[i].SortOrder = i + 1;
        }
        await _db.SaveChangesAsync(cancellationToken);
    }
}
