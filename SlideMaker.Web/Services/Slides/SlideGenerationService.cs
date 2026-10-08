using System.Text.Json;
using SlideMaker.Web.DTOs;
using SlideMaker.Web.Models;

namespace SlideMaker.Web.Services.Slides;

public class SlideGenerationService : ISlideGenerationService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public List<Slide> GenerateSlides(List<Question> questions, Template template, SlidePattern pattern, PresentationBrandingDto? branding = null)
    {
        var layout = DeserializeLayout(template.LayoutJson);
        var slides = new List<Slide>();
        var slideNumber = 0;

        switch (pattern)
        {
            case SlidePattern.MultipleQuestionsPerSlide:
                const int questionsPerSlide = 4;
                for (var i = 0; i < questions.Count; i += questionsPerSlide)
                {
                    slideNumber++;
                    var batch = questions.Skip(i).Take(questionsPerSlide).ToList();
                    slides.Add(BuildMultiQuestionSlide(batch, slideNumber, layout));
                }
                break;

            case SlidePattern.QuestionAndAnswerSameSlide:
                foreach (var question in questions)
                {
                    slideNumber++;
                    slides.Add(BuildQuestionSlide(question, slideNumber, layout, includeAnswer: true));
                }
                break;

            case SlidePattern.QuestionThenSolution:
                foreach (var question in questions)
                {
                    slideNumber++;
                    slides.Add(BuildQuestionSlide(question, slideNumber, layout, includeAnswer: false));
                    slideNumber++;
                    slides.Add(BuildSolutionSlide(question, slideNumber, layout));
                }
                break;

            case SlidePattern.OneQuestionPerSlide:
            default:
                foreach (var question in questions)
                {
                    slideNumber++;
                    slides.Add(BuildQuestionSlide(question, slideNumber, layout, includeAnswer: false));
                }
                break;
        }

        if (branding is not null)
        {
            foreach (var slide in slides)
            {
                AddBrandingElements(slide, layout, branding);
            }
        }

        return slides;
    }

    private static void AddBrandingElements(Slide slide, TemplateLayoutDto layout, PresentationBrandingDto branding)
    {
        var sortOrder = slide.Elements.Count == 0 ? 0 : slide.Elements.Max(e => e.SortOrder) + 1;

        // Top-left: academy name + class (always present — AcademyName defaults from config).
        var brandText = string.IsNullOrWhiteSpace(branding.ClassName)
            ? branding.AcademyName
            : $"{branding.AcademyName} · {branding.ClassName}";
        slide.Elements.Add(new SlideElement
        {
            ElementType = SlideElementType.Heading,
            Content = brandText,
            PositionX = 2, PositionY = 1.5, Width = 55, Height = 6,
            FontSize = 20, FontFamily = layout.FontFamily, Alignment = "left",
            TextColor = layout.AccentColor, Bold = true, SortOrder = sortOrder++
        });

        // Top-right: special-day / date badge.
        if (branding.EventDate.HasValue)
        {
            var dateText = string.IsNullOrWhiteSpace(branding.FestivalName)
                ? branding.EventDate.Value.ToString("dd MMM yyyy")
                : $"{branding.FestivalName} · {branding.EventDate.Value:dd MMM yyyy}";
            slide.Elements.Add(new SlideElement
            {
                ElementType = SlideElementType.Heading,
                Content = dateText,
                PositionX = 57, PositionY = 1.5, Width = 41, Height = 6,
                FontSize = 18, FontFamily = layout.FontFamily, Alignment = "right",
                TextColor = layout.TextColor, SortOrder = sortOrder++
            });
        }

        // Thin banner just under the header — free-text announcement.
        if (!string.IsNullOrWhiteSpace(branding.HeaderMessage))
        {
            slide.Elements.Add(new SlideElement
            {
                ElementType = SlideElementType.Footer,
                Content = branding.HeaderMessage,
                PositionX = 2, PositionY = 7.5, Width = 96, Height = 4,
                FontSize = 16, FontFamily = layout.FontFamily, Alignment = "center",
                TextColor = layout.AccentColor, SortOrder = sortOrder++
            });
        }

        // Bottom credit line (e.g. "Math by Shubham Sir").
        if (!string.IsNullOrWhiteSpace(branding.InstructorCredit))
        {
            slide.Elements.Add(new SlideElement
            {
                ElementType = SlideElementType.Footer,
                Content = branding.InstructorCredit,
                PositionX = 2, PositionY = 94, Width = 96, Height = 5,
                FontSize = 16, FontFamily = layout.FontFamily, Alignment = "center",
                TextColor = layout.TextColor, SortOrder = sortOrder
            });
        }
    }

    /// <summary>
    /// Picks the largest font size (down to a floor) that keeps the text within the template's
    /// intended box height; if even the smallest size doesn't fit, the box grows instead of
    /// clipping. Character-width estimate is deliberately conservative (mixed Latin/Devanagari
    /// text runs narrower per-line than pure Latin) — better to slightly over-allocate height
    /// than to clip a line.
    /// </summary>
    private static (double fontSize, double heightPct) FitTextBox(string text, double maxFontSize, double widthPct, double targetHeightPct, double cardPaddingPx)
    {
        const double minFontSize = 14;
        const double slideHeightPx = 540; // matches the 960x540 canvas used by export/preview
        const double charsPerLineFactor = 13; // conservative chars-per-line per point of font size, at widthPct=100
        const double lineHeightMultiplier = 1.35;

        var length = text.Length;

        double bestFont = minFontSize;
        double bestHeightPct = EstimateHeightPct(minFontSize);

        for (var candidate = maxFontSize; candidate >= minFontSize; candidate -= 1)
        {
            var heightPct = EstimateHeightPct(candidate);
            if (heightPct <= targetHeightPct)
            {
                bestFont = candidate;
                bestHeightPct = heightPct;
                break;
            }
            // Keep the smallest-font estimate as the fallback in case nothing fits the target.
            bestFont = candidate;
            bestHeightPct = heightPct;
        }

        return (bestFont, Math.Max(targetHeightPct, bestHeightPct));

        double EstimateHeightPct(double fontSize)
        {
            var charsPerLine = Math.Max(1, widthPct * charsPerLineFactor / fontSize);
            var lines = Math.Max(1, Math.Ceiling(length / charsPerLine));
            var neededPx = lines * fontSize * lineHeightMultiplier + (2 * cardPaddingPx) + 8;
            return neededPx / slideHeightPx * 100;
        }
    }

    private static TemplateLayoutDto DeserializeLayout(string layoutJson)
    {
        try
        {
            return JsonSerializer.Deserialize<TemplateLayoutDto>(layoutJson, JsonOptions) ?? new TemplateLayoutDto();
        }
        catch (JsonException)
        {
            return new TemplateLayoutDto();
        }
    }

    private static Slide BuildQuestionSlide(Question question, int slideNumber, TemplateLayoutDto layout, bool includeAnswer)
    {
        var slide = new Slide
        {
            SlideNumber = slideNumber,
            SortOrder = slideNumber,
            SlideType = SlideType.Question,
            Title = $"Question {question.SortOrder}",
            Content = question.QuestionText,
            Background = layout.Background,
            QuestionId = question.Id == 0 ? null : question.Id
        };

        // Long questions shrink the font and/or grow the box rather than clipping — see
        // FitTextBox. Options then start right after wherever the question box actually ends.
        var questionPos = layout.QuestionPosition;
        var questionCardPadding = string.IsNullOrWhiteSpace(layout.QuestionBackground) ? 0 : layout.CardPadding;
        var (questionFontSize, questionHeight) = FitTextBox(question.QuestionText, layout.TitleFontSize, questionPos.Width, questionPos.Height, questionCardPadding);

        slide.Elements.Add(NewElement(SlideElementType.QuestionText, question.QuestionText,
            new LayoutRegionDto { X = questionPos.X, Y = questionPos.Y, Width = questionPos.Width, Height = questionHeight },
            layout, questionFontSize, 0, layout.TextColor, layout.QuestionBackground, bold: true));

        var optionsText = string.Join("\n", question.Options.OrderBy(o => o.SortOrder)
            .Select(o => $"{o.OptionLabel}. {o.OptionText}"));
        if (!string.IsNullOrWhiteSpace(optionsText))
        {
            var originalOptionsPos = layout.OptionsPosition;
            var optionsBottom = originalOptionsPos.Y + originalOptionsPos.Height;
            var optionsTop = questionPos.Y + questionHeight + 2; // small gap below the question box
            var optionsPos = new LayoutRegionDto
            {
                X = originalOptionsPos.X,
                Y = optionsTop,
                Width = originalOptionsPos.Width,
                Height = Math.Max(10, optionsBottom - optionsTop)
            };

            slide.Elements.Add(NewElement(SlideElementType.OptionsList, optionsText, optionsPos, layout,
                layout.BodyFontSize, 1, layout.TextColor, layout.OptionsBackground));
        }

        if (includeAnswer)
        {
            if (!string.IsNullOrWhiteSpace(question.CorrectAnswer))
            {
                slide.Elements.Add(NewElement(SlideElementType.AnswerText, $"Answer: {question.CorrectAnswer}", layout.AnswerPosition, layout,
                    layout.BodyFontSize, 2, layout.AnswerTextColor ?? layout.AccentColor, layout.AnswerBackground, bold: true));
            }
            if (!string.IsNullOrWhiteSpace(question.Explanation))
            {
                slide.Elements.Add(NewElement(SlideElementType.ExplanationText, question.Explanation!, layout.ExplanationPosition, layout,
                    layout.BodyFontSize, 3, layout.TextColor, layout.ExplanationBackground));
            }
        }

        return slide;
    }

    private static Slide BuildSolutionSlide(Question question, int slideNumber, TemplateLayoutDto layout)
    {
        var slide = new Slide
        {
            SlideNumber = slideNumber,
            SortOrder = slideNumber,
            SlideType = SlideType.Answer,
            Title = $"Answer {question.SortOrder}",
            Content = question.CorrectAnswer,
            Background = layout.Background,
            QuestionId = question.Id == 0 ? null : question.Id
        };

        slide.Elements.Add(NewElement(SlideElementType.AnswerText, $"Answer: {question.CorrectAnswer}", layout.AnswerPosition, layout,
            layout.TitleFontSize, 0, layout.AnswerTextColor ?? layout.AccentColor, layout.AnswerBackground, bold: true));

        if (!string.IsNullOrWhiteSpace(question.Explanation))
        {
            slide.Elements.Add(NewElement(SlideElementType.ExplanationText, question.Explanation!, layout.ExplanationPosition, layout,
                layout.BodyFontSize, 1, layout.TextColor, layout.ExplanationBackground));
        }

        return slide;
    }

    private static Slide BuildMultiQuestionSlide(List<Question> batch, int slideNumber, TemplateLayoutDto layout)
    {
        var content = string.Join("\n\n", batch.Select(q => $"{q.SortOrder}. {q.QuestionText}"));

        var slide = new Slide
        {
            SlideNumber = slideNumber,
            SortOrder = slideNumber,
            SlideType = SlideType.Content,
            Title = "Questions",
            Content = content,
            Background = layout.Background
        };

        slide.Elements.Add(NewElement(SlideElementType.QuestionText, content, layout.QuestionPosition, layout,
            layout.BodyFontSize, 0, layout.TextColor, layout.QuestionBackground));
        return slide;
    }

    private static SlideElement NewElement(SlideElementType type, string content, LayoutRegionDto position,
        TemplateLayoutDto layout, double fontSize, int sortOrder, string? textColor, string? background, bool bold = false) => new()
    {
        ElementType = type,
        Content = content,
        PositionX = position.X,
        PositionY = position.Y,
        Width = position.Width,
        Height = position.Height,
        FontSize = fontSize,
        FontFamily = layout.FontFamily,
        Alignment = "left",
        TextColor = textColor,
        Background = string.IsNullOrWhiteSpace(background) ? null : background,
        BorderRadius = string.IsNullOrWhiteSpace(background) ? null : layout.BorderRadius,
        Padding = string.IsNullOrWhiteSpace(background) ? null : layout.CardPadding,
        Bold = bold,
        SortOrder = sortOrder
    };
}
