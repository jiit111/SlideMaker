namespace SlideMaker.Web.Services.SpecialDay;

/// <summary>
/// Suggests a festival/special-day name for a given date, so the user can build a themed
/// presentation (e.g. "Aaj Holi hai — Holi quiz banao"). This is a best-effort, hardcoded
/// calendar for Phase 1 (no live internet/calendar API access); the interface is kept
/// separate so a real calendar/web-search provider can replace it later without touching
/// callers. Users can always override or clear the suggested name.
/// </summary>
public interface ISpecialDayService
{
    string? GetSpecialDayName(DateTime date);
}
