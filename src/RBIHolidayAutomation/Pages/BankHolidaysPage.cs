using Microsoft.Playwright;
using RBIHolidayAutomation.Models;

namespace RBIHolidayAutomation.Pages;

/// <summary>
/// Page Object for the RBI Bank Holidays page.
/// </summary>
public sealed class BankHolidaysPage
{
    private readonly IPage _page;

    private const string BankHolidaysUrl = "https://rbi.org.in/Scripts/HolidayMatrixDisplay.aspx";

    private ILocator RegionalOfficeSelect => _page.Locator("#drRegionalOffice");
    private ILocator MonthSelect => _page.Locator("#drMonth");
    private ILocator YearSelect => _page.Locator("#drYear");
    private ILocator GoButton => _page.Locator("#btnGo");

    /// <summary>
    /// The holiday list table is the table that contains the holiday data.
    /// It is identified by containing month header rows and day/occasion data rows.
    /// Using a semantic approach: find the table that has rows with month names and day columns.
    /// </summary>
    private ILocator HolidayListTable => _page.Locator("table.tablebg").Filter(new LocatorFilterOptions
    {
        Has = _page.Locator("tr:has(th:only-child, td:only-child)")
    }).Last;

    public BankHolidaysPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Opens the Bank Holidays page directly.
    /// </summary>
    public async Task OpenAsync()
    {
        await _page.GotoAsync(BankHolidaysUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 60_000
        });
    }

    /// <summary>
    /// Selects the given regional office, month and year, then clicks GO.
    /// </summary>
    public async Task SearchHolidaysAsync(string regionalOffice, string month, string year)
    {
        await RegionalOfficeSelect.SelectOptionAsync(new SelectOptionValue { Label = regionalOffice });
        await MonthSelect.SelectOptionAsync(new SelectOptionValue { Label = month });
        await YearSelect.SelectOptionAsync(new SelectOptionValue { Label = year });
        await GoButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Extracts all holiday records from the results table.
    /// </summary>
    public async Task<IReadOnlyList<HolidayRecord>> ExtractHolidaysAsync(int year)
    {
        await HolidayListTable.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var rows = await HolidayListTable.Locator("tr").AllAsync();
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("The holiday results table is empty.");
        }

        var holidays = new List<HolidayRecord>();
        string? currentMonth = null;

        foreach (var row in rows)
        {
            var cells = await row.Locator("th, td").AllAsync();
            if (cells.Count == 0)
            {
                continue;
            }

            // Month header row: single cell containing the month name
            if (cells.Count == 1)
            {
                var monthText = (await cells[0].TextContentAsync())?.Trim();
                if (!string.IsNullOrWhiteSpace(monthText) && IsMonthName(monthText))
                {
                    currentMonth = monthText;
                }
                continue;
            }

            // Data row: Day | Occasion | Holiday type
            if (cells.Count >= 2 && currentMonth is not null)
            {
                var dayText = (await cells[0].TextContentAsync())?.Trim() ?? string.Empty;
                var occasionText = (await cells[1].TextContentAsync())?.Trim() ?? string.Empty;

                if (!int.TryParse(dayText, out var day))
                {
                    continue;
                }

                var occasion = NormalizeOccasion(occasionText);
                if (string.IsNullOrWhiteSpace(occasion))
                {
                    continue;
                }

                holidays.Add(new HolidayRecord
                {
                    Month = currentMonth,
                    Day = day,
                    Occasion = occasion,
                    Year = year,
                    Date = TryCreateDate(currentMonth, day, year)
                });
            }
        }

        return holidays;
    }

    private static bool IsMonthName(string text) =>
        DateTime.TryParseExact(text, "MMMM", System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out _);

    private static string NormalizeOccasion(string occasion)
    {
        // Collapse multiple whitespace characters and trim
        return System.Text.RegularExpressions.Regex.Replace(occasion, @"\s+", " ").Trim();
    }

    private static DateOnly? TryCreateDate(string month, int day, int year)
    {
        var monthNumber = Array.FindIndex(
            System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.MonthNames,
            m => string.Equals(m, month, StringComparison.OrdinalIgnoreCase)) + 1;

        if (monthNumber <= 0)
        {
            return null;
        }

        try
        {
            return new DateOnly(year, monthNumber, day);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}