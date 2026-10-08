namespace SlideMaker.Web.Areas.SlideMaker.ViewModels;

public class TemplateListViewModel
{
    public List<TemplateCardViewModel> Templates { get; set; } = new();
}

public class TemplateCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PreviewImagePath { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
}

public class TemplateFormViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string LayoutJson { get; set; } = string.Empty;
}

public class UploadImageViewModel
{
    public int PresentationId { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TemplateUploadViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ErrorMessage { get; set; }
}
