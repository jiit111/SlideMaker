namespace SlideMaker.Web.Services.Search;

public class SearchResultItem
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class SearchResults
{
    public List<SearchResultItem> Questions { get; set; } = new();
    public List<SearchResultItem> Templates { get; set; } = new();
    public List<SearchResultItem> Presentations { get; set; } = new();
}

public interface ISearchService
{
    Task<SearchResults> SearchAsync(string term, CancellationToken cancellationToken = default);
}
