using System.Text;
using RBIHolidayAutomation.Models;

namespace RBIHolidayAutomation.Services;

/// <summary>
/// Writes the holiday report to a text file.
/// </summary>
public sealed class HolidayReportWriter
{
    private const string ReportHeader = "==================================================";

    /// <summary>
    /// Generates the full report for all regional offices and writes it to the given path.
    /// </summary>
    public async Task WriteReportAsync(
        string reportPath,
        int year,
        IReadOnlyList<(string RegionalOffice, IReadOnlyList<HolidayRecord> Holidays, HolidaySummary Summary)> sections)
    {
        var sb = new StringBuilder();

        sb.AppendLine(ReportHeader);
        sb.AppendLine("RBI BANK HOLIDAY AUTOMATION REPORT");
        sb.AppendLine($"Year: {year}");
        sb.AppendLine(ReportHeader);
        sb.AppendLine();

        foreach (var section in sections)
        {
            AppendSection(sb, section.RegionalOffice, section.Holidays, section.Summary);
        }

        var directory = Path.GetDirectoryName(reportPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(reportPath, sb.ToString(), Encoding.UTF8);
    }

    private static void AppendSection(
        StringBuilder sb,
        string regionalOffice,
        IReadOnlyList<HolidayRecord> holidays,
        HolidaySummary summary)
    {
        sb.AppendLine();
        sb.AppendLine(ReportHeader);
        sb.AppendLine($"REGIONAL OFFICE: {regionalOffice.ToUpperInvariant()}");
        sb.AppendLine(ReportHeader);
        sb.AppendLine();
        sb.AppendLine("Month,Day,Occasion");

        foreach (var holiday in holidays)
        {
            sb.AppendLine($"{holiday.Month},{holiday.Day},{EscapeOccasion(holiday.Occasion)}");
        }

        sb.AppendLine();
        sb.AppendLine("---------------- SUMMARY ----------------");
        sb.AppendLine();
        sb.AppendLine($"Total holidays: {summary.TotalHolidays}");
        sb.AppendLine($"Month with highest number of holidays: {summary.HighestHolidayMonth} ({summary.HighestHolidayMonthCount})");
        sb.AppendLine($"Long weekends (3+ continuous non-working days): {summary.LongWeekendCount}");
        sb.AppendLine($"Longest continuous holiday stretch: {summary.LongestHolidayStretch} days");
        sb.AppendLine();
    }

    /// <summary>
    /// Wraps the occasion in double quotes if it contains a comma.
    /// </summary>
    private static string EscapeOccasion(string occasion) =>
        occasion.Contains(',') ? $"\"{occasion}\"" : occasion;
}