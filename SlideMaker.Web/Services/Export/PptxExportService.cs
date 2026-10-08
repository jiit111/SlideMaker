using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using SlideMaker.Web.Models;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

namespace SlideMaker.Web.Services.Export;

/// <summary>
/// Generates a real, editable .pptx via the free DocumentFormat.OpenXml SDK. This is a
/// "close visual match" export (solid colors only, simple rectangle/rounded-rectangle text
/// boxes) rather than a pixel-perfect copy of the HTML preview — good enough to open directly
/// in PowerPoint/Google Slides and keep editing.
/// </summary>
public class PptxExportService : IPptxExportService
{
    private const long SlideWidthEmu = 12192000; // 16:9 at 13.333in
    private const long SlideHeightEmu = 6858000;
    private const string DefaultTextHex = "1A1A2E";
    private const string DefaultBackgroundHex = "FFFFFF";

    private readonly ILogger<PptxExportService> _logger;

    public PptxExportService(ILogger<PptxExportService> logger)
    {
        _logger = logger;
    }

    public Task<ExportResult> ExportToPptxAsync(Presentation presentation, CancellationToken cancellationToken = default)
    {
        try
        {
            using var stream = new MemoryStream();
            using (var doc = PresentationDocument.Create(stream, PresentationDocumentType.Presentation))
            {
                var presentationPart = doc.AddPresentationPart();
                presentationPart.Presentation = new P.Presentation();

                var (slideMasterPart, slideLayoutPart) = CreateSlideMaster(presentationPart);

                var slideIdList = new P.SlideIdList();
                uint slideId = 256;
                var slides = presentation.Slides.OrderBy(s => s.SortOrder).ToList();

                foreach (var slide in slides)
                {
                    var slidePart = presentationPart.AddNewPart<SlidePart>();
                    slidePart.Slide = BuildSlide(slide);
                    slidePart.AddPart(slideLayoutPart);

                    var relationshipId = presentationPart.GetIdOfPart(slidePart);
                    slideIdList.Append(new P.SlideId { Id = slideId, RelationshipId = relationshipId });
                    slideId++;
                }

                presentationPart.Presentation.Append(
                    new P.SlideMasterIdList(new P.SlideMasterId
                    {
                        Id = 2147483648U,
                        RelationshipId = presentationPart.GetIdOfPart(slideMasterPart)
                    }),
                    slideIdList,
                    new P.SlideSize { Cx = (Int32Value)(int)SlideWidthEmu, Cy = (Int32Value)(int)SlideHeightEmu },
                    new P.NotesSize { Cx = 6858000, Cy = 9144000 });

                presentationPart.Presentation.Save();
            }

            return Task.FromResult(new ExportResult
            {
                Success = true,
                FileBytes = stream.ToArray(),
                FileName = $"{SanitizeFileName(presentation.Name)}.pptx",
                ContentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PPTX export failed for presentation {Id}", presentation.Id);
            return Task.FromResult(new ExportResult { Success = false, ErrorMessage = "Could not generate the PPTX file." });
        }
    }

    private static (SlideMasterPart master, SlideLayoutPart layout) CreateSlideMaster(PresentationPart presentationPart)
    {
        var slideMasterPart = presentationPart.AddNewPart<SlideMasterPart>();
        var slideLayoutPart = slideMasterPart.AddNewPart<SlideLayoutPart>();

        slideLayoutPart.SlideLayout = new P.SlideLayout(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties { Id = 1, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new P.ApplicationNonVisualDrawingProperties()),
                    new P.GroupShapeProperties(new A.TransformGroup()))),
            new P.ColorMapOverride(new A.MasterColorMapping()))
        { Type = P.SlideLayoutValues.Blank };

        slideMasterPart.SlideMaster = new P.SlideMaster(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties { Id = 1, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new P.ApplicationNonVisualDrawingProperties()),
                    new P.GroupShapeProperties(new A.TransformGroup()))),
            new P.ColorMap
            {
                Background1 = A.ColorSchemeIndexValues.Light1,
                Text1 = A.ColorSchemeIndexValues.Dark1,
                Background2 = A.ColorSchemeIndexValues.Light2,
                Text2 = A.ColorSchemeIndexValues.Dark2,
                Accent1 = A.ColorSchemeIndexValues.Accent1,
                Accent2 = A.ColorSchemeIndexValues.Accent2,
                Accent3 = A.ColorSchemeIndexValues.Accent3,
                Accent4 = A.ColorSchemeIndexValues.Accent4,
                Accent5 = A.ColorSchemeIndexValues.Accent5,
                Accent6 = A.ColorSchemeIndexValues.Accent6,
                Hyperlink = A.ColorSchemeIndexValues.Hyperlink,
                FollowedHyperlink = A.ColorSchemeIndexValues.FollowedHyperlink
            },
            new P.SlideLayoutIdList(new P.SlideLayoutId
            {
                Id = 2147483649U,
                RelationshipId = slideMasterPart.GetIdOfPart(slideLayoutPart)
            }));

        AddTheme(slideMasterPart);

        return (slideMasterPart, slideLayoutPart);
    }

    private static void AddTheme(SlideMasterPart slideMasterPart)
    {
        var themePart = slideMasterPart.AddNewPart<ThemePart>();
        var colorScheme = new A.ColorScheme(
            new A.Dark1Color(new A.SystemColor { Val = A.SystemColorValues.WindowText }),
            new A.Light1Color(new A.SystemColor { Val = A.SystemColorValues.Window }),
            new A.Dark2Color(new A.RgbColorModelHex { Val = "1A1A2E" }),
            new A.Light2Color(new A.RgbColorModelHex { Val = "EEECE1" }),
            new A.Accent1Color(new A.RgbColorModelHex { Val = "00E5A0" }),
            new A.Accent2Color(new A.RgbColorModelHex { Val = "7C5CFC" }),
            new A.Accent3Color(new A.RgbColorModelHex { Val = "0D6EFD" }),
            new A.Accent4Color(new A.RgbColorModelHex { Val = "FFC107" }),
            new A.Accent5Color(new A.RgbColorModelHex { Val = "DC3545" }),
            new A.Accent6Color(new A.RgbColorModelHex { Val = "20C997" }),
            new A.Hyperlink(new A.RgbColorModelHex { Val = "0563C1" }),
            new A.FollowedHyperlinkColor(new A.RgbColorModelHex { Val = "954F72" }))
        { Name = "SlideMaker" };

        var fontScheme = new A.FontScheme(
            new A.MajorFont(new A.LatinFont { Typeface = "Segoe UI" }, new A.EastAsianFont { Typeface = "" }, new A.ComplexScriptFont { Typeface = "" }),
            new A.MinorFont(new A.LatinFont { Typeface = "Segoe UI" }, new A.EastAsianFont { Typeface = "" }, new A.ComplexScriptFont { Typeface = "" }))
        { Name = "SlideMaker" };

        var formatScheme = new A.FormatScheme(
            new A.FillStyleList(
                new A.SolidFill(new A.SchemeColor { Val = A.SchemeColorValues.Accent1 }),
                new A.SolidFill(new A.SchemeColor { Val = A.SchemeColorValues.Accent1 }),
                new A.SolidFill(new A.SchemeColor { Val = A.SchemeColorValues.Accent1 })),
            new A.LineStyleList(
                new A.Outline(new A.SolidFill(new A.SchemeColor { Val = A.SchemeColorValues.Text1 })),
                new A.Outline(new A.SolidFill(new A.SchemeColor { Val = A.SchemeColorValues.Text1 })),
                new A.Outline(new A.SolidFill(new A.SchemeColor { Val = A.SchemeColorValues.Text1 }))),
            new A.EffectStyleList(
                new A.EffectStyle(new A.EffectList()),
                new A.EffectStyle(new A.EffectList()),
                new A.EffectStyle(new A.EffectList())),
            new A.BackgroundFillStyleList(
                new A.SolidFill(new A.SchemeColor { Val = A.SchemeColorValues.Light1 }),
                new A.SolidFill(new A.SchemeColor { Val = A.SchemeColorValues.Light1 }),
                new A.SolidFill(new A.SchemeColor { Val = A.SchemeColorValues.Light1 })))
        { Name = "SlideMaker" };

        themePart.Theme = new A.Theme(
            new A.ThemeElements(colorScheme, fontScheme, formatScheme),
            new A.ObjectDefaults(),
            new A.ExtraColorSchemeList())
        { Name = "SlideMaker" };
    }

    private static P.Slide BuildSlide(Slide slide)
    {
        var shapeTree = new P.ShapeTree(
            new P.NonVisualGroupShapeProperties(
                new P.NonVisualDrawingProperties { Id = 1, Name = "" },
                new P.NonVisualGroupShapeDrawingProperties(),
                new P.ApplicationNonVisualDrawingProperties()),
            new P.GroupShapeProperties(new A.TransformGroup()));

        var backgroundHex = PptxColorHelper.ResolveHex(slide.Background, DefaultBackgroundHex);
        shapeTree.Append(BuildRectangle(2, "Background", 0, 0, 100, 100, backgroundHex, roundRadiusPct: 0));

        uint shapeId = 3;
        foreach (var element in slide.Elements.OrderBy(e => e.SortOrder))
        {
            if (element.Background is { Length: > 0 })
            {
                var cardHex = PptxColorHelper.ResolveHex(element.Background, backgroundHex);
                shapeTree.Append(BuildRectangle(shapeId++, $"Card {shapeId}", element.PositionX, element.PositionY,
                    element.Width, element.Height, cardHex, roundRadiusPct: element.BorderRadius is > 0 ? 6 : 0));
            }

            shapeTree.Append(BuildTextBox(shapeId++, element));
        }

        return new P.Slide(new P.CommonSlideData(shapeTree), new P.ColorMapOverride(new A.MasterColorMapping()));
    }

    private static P.Shape BuildRectangle(uint id, string name, double xPct, double yPct, double wPct, double hPct,
        string fillHex, double roundRadiusPct)
    {
        var geometry = roundRadiusPct > 0
            ? new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.RoundRectangle }
            : new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle };

        return new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties { Id = id, Name = name },
                new P.NonVisualShapeDrawingProperties(new A.ShapeLocks { NoGrouping = true }),
                new P.ApplicationNonVisualDrawingProperties()),
            new P.ShapeProperties(
                ToTransform(xPct, yPct, wPct, hPct),
                geometry,
                new A.SolidFill(new A.RgbColorModelHex { Val = fillHex }),
                new A.Outline(new A.NoFill())),
            new P.TextBody(new A.BodyProperties(), new A.ListStyle(), new A.Paragraph()));
    }

    private static P.Shape BuildTextBox(uint id, SlideElement element)
    {
        var textColorHex = PptxColorHelper.ResolveHex(element.TextColor, DefaultTextHex);
        var fontSizePt = (int)Math.Round(element.FontSize ?? 20);
        var alignment = MapAlignment(element.Alignment);
        var lines = element.Content.Replace("\r\n", "\n").Split('\n');

        var textBody = new P.TextBody(
            new A.BodyProperties { Wrap = A.TextWrappingValues.Square, Anchor = A.TextAnchoringTypeValues.Top },
            new A.ListStyle());

        foreach (var line in lines)
        {
            var run = new A.Run(
                new A.RunProperties { Language = "en-US", FontSize = fontSizePt * 100, Bold = element.Bold, Dirty = false }
                    .WithSolidFill(textColorHex),
                new A.Text(line));

            textBody.Append(new A.Paragraph(new A.ParagraphProperties { Alignment = alignment }, run));
        }

        return new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties { Id = id, Name = $"Text {id}" },
                new P.NonVisualShapeDrawingProperties(new A.ShapeLocks { NoGrouping = true }),
                new P.ApplicationNonVisualDrawingProperties()),
            new P.ShapeProperties(
                ToTransform(element.PositionX, element.PositionY, element.Width, element.Height),
                new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle },
                new A.NoFill(),
                new A.Outline(new A.NoFill())),
            textBody);
    }

    private static A.Transform2D ToTransform(double xPct, double yPct, double wPct, double hPct) => new(
        new A.Offset { X = (long)(xPct / 100 * SlideWidthEmu), Y = (long)(yPct / 100 * SlideHeightEmu) },
        new A.Extents { Cx = (long)(wPct / 100 * SlideWidthEmu), Cy = (long)(hPct / 100 * SlideHeightEmu) });

    private static A.TextAlignmentTypeValues MapAlignment(string? alignment) => alignment?.ToLowerInvariant() switch
    {
        "center" => A.TextAlignmentTypeValues.Center,
        "right" => A.TextAlignmentTypeValues.Right,
        "justify" => A.TextAlignmentTypeValues.Justified,
        _ => A.TextAlignmentTypeValues.Left
    };

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Where(c => !invalid.Contains(c)).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "presentation" : cleaned;
    }
}

internal static class RunPropertiesExtensions
{
    public static A.RunProperties WithSolidFill(this A.RunProperties props, string hex)
    {
        props.Append(new A.SolidFill(new A.RgbColorModelHex { Val = hex }));
        return props;
    }
}
