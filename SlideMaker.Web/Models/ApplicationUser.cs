namespace SlideMaker.Web.Models;

public class ApplicationUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }

    public ICollection<Presentation> Presentations { get; set; } = new List<Presentation>();
    public ICollection<Template> Templates { get; set; } = new List<Template>();
}
