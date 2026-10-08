using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SlideMaker.Web.Data;
using SlideMaker.Web.Data.Seed;
using SlideMaker.Web.Middleware;
using SlideMaker.Web.Options;
using SlideMaker.Web.Services.AI;
using SlideMaker.Web.Services.Export;
using SlideMaker.Web.Services.OCR;
using SlideMaker.Web.Services.Presentations;
using SlideMaker.Web.Services.Prompts;
using SlideMaker.Web.Services.Questions;
using SlideMaker.Web.Services.Search;
using SlideMaker.Web.Services.Slides;
using SlideMaker.Web.Services.SpecialDay;
using SlideMaker.Web.Services.Storage;
using SlideMaker.Web.Services.Templates;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Strongly-typed configuration (no hardcoded connection strings / keys / paths).
builder.Services.Configure<AIOptions>(builder.Configuration.GetSection(AIOptions.SectionName));
builder.Services.Configure<OcrOptions>(builder.Configuration.GetSection(OcrOptions.SectionName));
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection(FileStorageOptions.SectionName));
builder.Services.Configure<BrandingOptions>(builder.Configuration.GetSection(BrandingOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useSqlite = string.IsNullOrWhiteSpace(connectionString);

// Production can use SQL Server through ConnectionStrings__DefaultConnection.
// A local SQLite database keeps single-container deployments operational when
// the hosting platform does not provide SQL Server.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (useSqlite)
    {
        var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDirectory);
        options.UseSqlite($"Data Source={Path.Combine(dataDirectory, "slidemaker.db")}");
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

// Infrastructure services.
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IOcrService, TesseractOcrService>();
builder.Services.AddScoped<IQuestionExtractionService, QuestionExtractionService>();
builder.Services.AddScoped<IPromptService, PromptService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<ISlideGenerationService, SlideGenerationService>();
builder.Services.AddScoped<IPresentationService, PresentationService>();
builder.Services.AddScoped<IExportService, HtmlExportService>();
builder.Services.AddScoped<IPptxExportService, PptxExportService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddSingleton<ISpecialDayService, SpecialDayService>();

// AI provider abstraction: three selectable implementations behind one IAIService,
// chosen at runtime via the "AI:Provider" setting (Mock | Ollama | OpenAICompatible).
builder.Services.AddHttpClient<OllamaAIService>();
builder.Services.AddHttpClient<OpenAICompatibleAIService>();
builder.Services.AddScoped<MockAIService>();
builder.Services.AddScoped<IAIService>(sp =>
{
    var provider = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AIOptions>>().Value.Provider;
    return provider switch
    {
        "Ollama" => sp.GetRequiredService<OllamaAIService>(),
        "OpenAICompatible" => sp.GetRequiredService<OpenAICompatibleAIService>(),
        _ => sp.GetRequiredService<MockAIService>()
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (useSqlite)
    {
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        await db.Database.MigrateAsync();
    }
    await DbSeeder.SeedAsync(db);
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
