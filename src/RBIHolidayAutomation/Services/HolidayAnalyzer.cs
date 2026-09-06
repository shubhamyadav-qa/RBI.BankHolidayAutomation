using RBIHolidayAutomation.Models;

namespace RBIHolidayAutomation.Services;

/// <summary>
/// Computes holiday statistics from a collection of holiday records.
/// </summary>
public sealed class HolidayAnalyzer
{
    /// <summary>
    /// Minimum number of consecutive non-working days that constitutes a "long weekend".
    /// </summary>
    private const int LongWeekendThreshold = 3;

    /// <summary>
    /// Calculates the summary statistics for the given holidays.
    /// </summary>
    public HolidaySummary Analyze(string regionalOffice, IReadOnlyList<HolidayRecord> holidays)
    {
        var totalHolidays = holidays.Count;

        var monthGroups = holidays
            .GroupBy(h => h.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ThenBy(g => g.Month)
            .ToList();

        var highestMonth = monthGroups.FirstOrDefault();
        var highestMonthName = highestMonth?.Month ?? string.Empty;
        var highestMonthCount = highestMonth?.Count ?? 0;

        var stretches = FindConsecutiveStretches(holidays);
        var longWeekendCount = stretches.Count(s => s >= LongWeekendThreshold);
        var longestStretch = stretches.Count > 0 ? stretches.Max() : 0;

        return new HolidaySummary
        {
            RegionalOffice = regionalOffice,
            TotalHolidays = totalHolidays,
            HighestHolidayMonth = highestMonthName,
            HighestHolidayMonthCount = highestMonthCount,
            LongWeekendCount = longWeekendCount,
            LongestHolidayStretch = longestStretch
        };
    }

    /// <summary>
    /// Groups holiday dates into consecutive stretches and returns the length of each stretch.
    /// </summary>
    private static List<int> FindConsecutiveStretches(IReadOnlyList<HolidayRecord> holidays)
    {
        var dates = holidays
            .Where(h => h.Date.HasValue)
            .Select(h => h.Date!.Value)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        if (dates.Count == 0)
        {
            return [];
        }

        var stretches = new List<int>();
        var currentStretch = 1;

        for (var i = 1; i < dates.Count; i++)
        {
            if (dates[i] == dates[i - 1].AddDays(1))
            {
                currentStretch++;
            }
            else
            {
                stretches.Add(currentStretch);
                currentStretch = 1;
            }
        }

        stretches.Add(currentStretch);
        return stretches;
    }
}