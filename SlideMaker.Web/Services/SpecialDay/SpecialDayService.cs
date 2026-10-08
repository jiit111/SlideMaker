namespace SlideMaker.Web.Services.SpecialDay;

/// <summary>
/// Best-effort hardcoded calendar (Phase 1 — no live calendar/internet API in this build).
/// Fixed-date observances apply to any year; lunar/shifting festivals are only covered for
/// 2025-2027 with approximate dates recalled at build time — please verify against a current
/// panchang/calendar before relying on them for an important event, and feel free to edit
/// this table directly. Users can always type/override the suggested name in the UI.
/// </summary>
public class SpecialDayService : ISpecialDayService
{
    // (Month, Day) -> name. Applies every year.
    private static readonly Dictionary<(int Month, int Day), string> FixedDates = new()
    {
        [(1, 1)] = "New Year's Day",
        [(1, 26)] = "Republic Day",
        [(3, 8)] = "International Women's Day",
        [(5, 1)] = "Labour Day",
        [(8, 15)] = "Independence Day",
        [(9, 5)] = "Teachers' Day",
        [(10, 2)] = "Gandhi Jayanti",
        [(11, 14)] = "Children's Day",
        [(12, 25)] = "Christmas"
    };

    // Lunar/shifting festivals — approximate, year-specific, best-effort only.
    private static readonly Dictionary<int, Dictionary<(int Month, int Day), string>> ShiftingDatesByYear = new()
    {
        [2025] = new()
        {
            [(3, 14)] = "Holi",
            [(8, 9)] = "Raksha Bandhan",
            [(8, 16)] = "Janmashtami",
            [(8, 27)] = "Ganesh Chaturthi",
            [(10, 2)] = "Dussehra (Vijayadashami)",
            [(10, 20)] = "Diwali",
            [(10, 22)] = "Bhai Dooj"
        },
        [2026] = new()
        {
            [(3, 4)] = "Holi",
            [(8, 28)] = "Raksha Bandhan",
            [(9, 4)] = "Janmashtami",
            [(9, 14)] = "Ganesh Chaturthi",
            [(10, 20)] = "Dussehra (Vijayadashami)",
            [(11, 8)] = "Diwali",
            [(11, 10)] = "Bhai Dooj"
        },
        [2027] = new()
        {
            [(3, 22)] = "Holi",
            [(8, 17)] = "Raksha Bandhan",
            [(10, 9)] = "Dussehra (Vijayadashami)",
            [(10, 29)] = "Diwali"
        }
    };

    public string? GetSpecialDayName(DateTime date)
    {
        var key = (date.Month, date.Day);

        if (ShiftingDatesByYear.TryGetValue(date.Year, out var yearMap) && yearMap.TryGetValue(key, out var shifting))
        {
            return shifting;
        }

        return FixedDates.TryGetValue(key, out var fixedName) ? fixedName : null;
    }
}
