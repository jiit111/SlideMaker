using SlideMaker.Web.DTOs;
using SlideMaker.Web.Models;

namespace SlideMaker.Web.Services.Slides;

public interface ISlideGenerationService
{
    /// <summary>
    /// Builds the Slide/SlideElement graph for a presentation from its selected questions,
    /// template (layout/fonts/colors), slide pattern, and branding chrome (academy/class/
    /// special-day/message banner). Template and content stay separate: the same questions
    /// can be re-run through this with a different template.
    /// </summary>
    List<Slide> GenerateSlides(List<Question> questions, Template template, SlidePattern pattern, PresentationBrandingDto? branding = null);
}
