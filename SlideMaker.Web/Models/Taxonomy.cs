namespace SlideMaker.Web.Models;

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
}

public class Chapter
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
    public string Name { get; set; } = string.Empty;

    public ICollection<Topic> Topics { get; set; } = new List<Topic>();
}

public class Topic
{
    public int Id { get; set; }
    public int ChapterId { get; set; }
    public Chapter Chapter { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
}
