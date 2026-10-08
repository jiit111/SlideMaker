namespace SlideMaker.Web.DTOs;

/// <summary>
/// The academy/class/special-day/message chrome baked into every slide of a presentation
/// (spec: header + calendar/festival + free-text banner + instructor credit).
/// </summary>
public class PresentationBrandingDto
{
    public string AcademyName { get; set; } = "PS Academy";
    public string? ClassName { get; set; }
    public DateTime? EventDate { get; set; }
    public string? FestivalName { get; set; }
    public string? HeaderMessage { get; set; }
    public string? InstructorCredit { get; set; }
}
