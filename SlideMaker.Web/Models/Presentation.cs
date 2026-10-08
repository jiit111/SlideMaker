namespace SlideMaker.Web.Models;

public class Presentation
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int? TemplateId { get; set; }
    public Template? Template { get; set; }

    public SlidePattern SlidePattern { get; set; } = SlidePattern.OneQuestionPerSlide;
    public PresentationStatus Status { get; set; } = PresentationStatus.Draft;

    // Branding chrome baked into every generated slide (§ header/calendar/message feature).
    public string? ClassName { get; set; }
    public DateTime? EventDate { get; set; }
    public string? FestivalName { get; set; }
    public string? HeaderMessage { get; set; }
    public string? InstructorCredit { get; set; }

    public int CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }

    public ICollection<Slide> Slides { get; set; } = new List<Slide>();
}

public class Slide
{
    public int Id { get; set; }
    public int PresentationId { get; set; }
    public Presentation Presentation { get; set; } = null!;

    public int SlideNumber { get; set; }
    public SlideType SlideType { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Background { get; set; }
    public string? Layout { get; set; }
    public int SortOrder { get; set; }

    /// <summary>Optional link back to the source question, when this slide represents one.</summary>
    public int? QuestionId { get; set; }
    public Question? Question { get; set; }

    public ICollection<SlideElement> Elements { get; set; } = new List<SlideElement>();
}

public class SlideElement
{
    public int Id { get; set; }
    public int SlideId { get; set; }
    public Slide Slide { get; set; } = null!;

    public SlideElementType ElementType { get; set; }
    public string Content { get; set; } = string.Empty;
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double? FontSize { get; set; }
    public string? FontFamily { get; set; }
    public string? Alignment { get; set; }
    public string? TextColor { get; set; }
    public string? Background { get; set; }
    public double? BorderRadius { get; set; }
    public double? Padding { get; set; }
    public bool Bold { get; set; }
    public int SortOrder { get; set; }
}
