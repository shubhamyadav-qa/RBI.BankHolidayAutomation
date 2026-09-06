namespace RBIHolidayAutomation.Models;

/// <summary>
/// Aggregated statistics computed from a collection of holiday records.
/// </summary>
public sealed record HolidaySummary
{
    public required string RegionalOffice { get; init; }

    public required int TotalHolidays { get; init; }

    public required string HighestHolidayMonth { get; init; }

    public required int HighestHolidayMonthCount { get; init; }

    public required int LongWeekendCount { get; init; }

    public required int LongestHolidayStretch { get; init; }
}