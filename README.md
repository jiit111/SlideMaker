# AI Slide Maker Platform — Phase 1

ASP.NET Core MVC (.NET 8) application. First module: **Slide Maker** — turn uploaded question
images, AI-generated questions, or existing question-bank items into a slide presentation.

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB is enough for local dev; `appsettings.Development.json` already points at
  `(localdb)\mssqllocaldb`)

## Run it

```
cd SlideMaker.Web
dotnet ef database update   # applies migrations + seeds a default user/subject/template
dotnet run
```

Then open the URL shown in the console. The dashboard's "AI Generate" flow works immediately
with **zero configuration** — Phase 1 defaults `AI:Provider` to `Mock`, which returns realistic
placeholder questions so the whole pipeline (generate → review → template → pattern → slides →
preview → export) can be exercised end to end without any API key.

## Configuring a real AI provider

Set `AI:Provider` in `appsettings.json` / `appsettings.Development.json` to one of:

- `Mock` (default) — no config needed.
- `Ollama` — run a local [Ollama](https://ollama.com) server and set `AI:Ollama:Endpoint` /
  `AI:Ollama:Model`.
- `OpenAICompatible` — any OpenAI-compatible chat-completions endpoint (OpenAI, Azure OpenAI, or
  a free-tier compatible provider). Set `AI:OpenAICompatible:Endpoint` / `ApiKey` / `Model`. The
  key is read server-side only and never reaches the browser.

## OCR (image → questions)

Uses the free, local Tesseract OCR engine. See `SlideMaker.Web/App_Data/tessdata/README.md` —
you need to drop `eng.traineddata`/`hin.traineddata` there once per machine. Until then, image
upload shows a friendly message and lets you enter questions manually; nothing crashes.

## What's implemented (Phase 1)

Dashboard, image upload → OCR → question extraction, AI question generation (form + freeform
"quick AI" box), question review/edit (add/delete/duplicate/split/merge/regenerate/reorder),
question bank search, template library (CRUD, set default, duplicate), slide generation with
4 configurable patterns, slide preview (prev/next, move/duplicate/delete slide), basic HTML
export, global search, and voice input via the browser's Web Speech API on every relevant text
field. See the code comments in `Services/*` for where Phase 2/3 items (PDF/DOCX upload, PPTX
template parsing, PPTX/PDF export, background job processing, special-day intelligence) are
meant to plug in.
