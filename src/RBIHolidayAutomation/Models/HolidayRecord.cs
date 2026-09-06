namespace RBIHolidayAutomation.Models;

/// <summary>
/// Represents a single holiday entry extracted from the RBI Bank Holidays table.
/// </summary>
public sealed record HolidayRecord
{
    public required string Month { get; init; }

    public required int Day { get; init; }

    public required string Occasion { get; init; }

    public required int Year { get; init; }

    /// <summary>
    /// The full holiday date. Null when the month/day cannot be resolved to a valid date.
    /// </summary>
    public DateOnly? Date { get; init; }
}