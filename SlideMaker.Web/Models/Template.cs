namespace SlideMaker.Web.Models;

public class Template
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>JSON-serialized layout metadata: fonts, colors, background, element positions per slide type.</summary>
    public string LayoutJson { get; set; } = string.Empty;

    public string? PreviewImagePath { get; set; }
    public string? SourceFilePath { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public int CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }

    public ICollection<Presentation> Presentations { get; set; } = new List<Presentation>();
}
