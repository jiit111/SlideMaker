using SlideMaker.Web.Models;

namespace SlideMaker.Web.Data.Seed;

/// <summary>Seeds a single default user (Phase 1 has no login UI) and a small sample taxonomy/template.</summary>
public static class DbSeeder
{
    public const int DefaultUserId = 1;

    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        if (!context.Users.Any())
        {
            context.Users.Add(new ApplicationUser
            {
                Name = "Default User",
                Email = "default@slidemaker.local",
                CreatedOn = DateTime.UtcNow
            });
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!context.Subjects.Any())
        {
            var science = new Subject { Name = "Science" };
            var math = new Subject { Name = "Mathematics" };
            context.Subjects.AddRange(science, math);
            await context.SaveChangesAsync(cancellationToken);

            var lifeProcesses = new Chapter { SubjectId = science.Id, Name = "Life Processes" };
            var lightChapter = new Chapter { SubjectId = science.Id, Name = "Light - Reflection and Refraction" };
            context.Chapters.AddRange(lifeProcesses, lightChapter);
            await context.SaveChangesAsync(cancellationToken);

            context.Topics.AddRange(
                new Topic { ChapterId = lifeProcesses.Id, Name = "Photosynthesis" },
                new Topic { ChapterId = lifeProcesses.Id, Name = "Respiration" },
                new Topic { ChapterId = lightChapter.Id, Name = "Reflection of Light" });
            await context.SaveChangesAsync(cancellationToken);
        }

        var hasAurora = context.Templates.Any(t => t.Name == "Aurora (Dark)");
        var hasClassic = context.Templates.Any(t => t.Name == "Classic Blue");

        if (!hasAurora)
        {
            // Aurora replaces the old flat "Classic Blue" default — existing databases had that
            // plain template marked default, which is exactly the "sada sada" export bug this fixes.
            foreach (var existing in context.Templates.Where(t => t.IsDefault))
            {
                existing.IsDefault = false;
            }

            context.Templates.Add(new Template
            {
                Name = "Aurora (Dark)",
                Description = "Vibrant dark-gradient template with glass-card panels and teal/violet accents.",
                LayoutJson = AuroraDarkLayout,
                IsDefault = true,
                IsActive = true,
                CreatedByUserId = DefaultUserId,
                CreatedOn = DateTime.UtcNow
            });
        }

        if (!hasClassic)
        {
            context.Templates.Add(new Template
            {
                Name = "Classic Blue",
                Description = "Simple, clean, printer-friendly template with a blue header band.",
                LayoutJson = ClassicBlueLayout,
                IsDefault = false,
                IsActive = true,
                CreatedByUserId = DefaultUserId,
                CreatedOn = DateTime.UtcNow
            });
        }

        if (!hasAurora || !hasClassic)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private const string ClassicBlueLayout = """
    {
        "background": "#ffffff",
        "headerColor": "#0d6efd",
        "accentColor": "#0d6efd",
        "fontFamily": "Segoe UI",
        "titleFontSize": 22,
        "bodyFontSize": 24,
        "textColor": "#1a1a2e",
        "questionPosition": { "x": 5, "y": 15, "width": 90, "height": 20 },
        "optionsPosition": { "x": 8, "y": 37, "width": 84, "height": 53 },
        "answerPosition": { "x": 5, "y": 20, "width": 90, "height": 20 },
        "explanationPosition": { "x": 5, "y": 45, "width": 90, "height": 40 }
    }
    """;

    private const string AuroraDarkLayout = """
    {
        "background": "linear-gradient(135deg, #0f0c29, #302b63, #24243e)",
        "headerColor": "#00e5a0",
        "accentColor": "#00e5a0",
        "fontFamily": "Segoe UI",
        "titleFontSize": 24,
        "bodyFontSize": 26,
        "textColor": "#f5f5f7",
        "questionBackground": "rgba(255,255,255,0.08)",
        "optionsBackground": "rgba(255,255,255,0.06)",
        "answerBackground": "rgba(0,229,160,0.16)",
        "answerTextColor": "#00e5a0",
        "explanationBackground": "rgba(255,255,255,0.05)",
        "borderRadius": 18,
        "cardPadding": 28,
        "questionPosition": { "x": 6, "y": 12, "width": 88, "height": 22 },
        "optionsPosition": { "x": 8, "y": 36, "width": 84, "height": 54 },
        "answerPosition": { "x": 6, "y": 20, "width": 88, "height": 22 },
        "explanationPosition": { "x": 6, "y": 46, "width": 88, "height": 42 }
    }
    """;
}
