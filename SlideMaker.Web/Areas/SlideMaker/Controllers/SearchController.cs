using Microsoft.AspNetCore.Mvc;
using SlideMaker.Web.Services.Search;

namespace SlideMaker.Web.Areas.SlideMaker.Controllers;

[Area("SlideMaker")]
public class SearchController : Controller
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, CancellationToken cancellationToken)
    {
        var results = string.IsNullOrWhiteSpace(q)
            ? new SearchResults()
            : await _searchService.SearchAsync(q, cancellationToken);

        ViewBag.Term = q;
        return View(results);
    }

    [HttpGet]
    public async Task<IActionResult> Suggest(string q, CancellationToken cancellationToken)
    {
        var results = string.IsNullOrWhiteSpace(q)
            ? new SearchResults()
            : await _searchService.SearchAsync(q, cancellationToken);
        return Json(results);
    }
}
