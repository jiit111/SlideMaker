using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SlideMaker.Web.Models;
using SlideMaker.Web.Services.Presentations;
using SlideMaker.Web.Services.Questions;
using SlideMaker.Web.Services.Templates;
using SlideMaker.Web.ViewModels;

namespace SlideMaker.Web.Controllers;

public class HomeController : Controller
{
    private readonly IPresentationService _presentationService;
    private readonly ITemplateService _templateService;
    private readonly IQuestionService _questionService;

    public HomeController(IPresentationService presentationService, ITemplateService templateService,
        IQuestionService questionService)
    {
        _presentationService = presentationService;
        _templateService = templateService;
        _questionService = questionService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var recent = await _presentationService.GetRecentAsync(10, cancellationToken);
        var templates = await _templateService.GetAllAsync(cancellationToken);
        var questionCount = await _questionService.CountAsync(cancellationToken);

        var viewModel = new DashboardViewModel
        {
            QuestionBankCount = questionCount,
            RecentPresentations = recent.Select(p => new PresentationSummaryViewModel
            {
                Id = p.Id,
                Name = p.Name,
                SlideCount = p.Slides.Count,
                CreatedOn = p.CreatedOn,
                TemplateName = p.Template?.Name,
                Status = p.Status
            }).ToList(),
            Templates = templates.Select(t => new TemplateSummaryViewModel
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                PreviewImagePath = t.PreviewImagePath,
                IsDefault = t.IsDefault
            }).ToList()
        };

        return View(viewModel);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
