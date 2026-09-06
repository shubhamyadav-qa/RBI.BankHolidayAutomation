using RBIHolidayAutomation.Models;
using RBIHolidayAutomation.Services;

namespace RBIHolidayAutomation.Tests;

[TestFixture]
public sealed class HolidayAnalyzerTests
{
    private HolidayAnalyzer _analyzer = null!;

    [SetUp]
    public void SetUp()
    {
        _analyzer = new HolidayAnalyzer();
    }

    [Test]
    public void Analyze_EmptyHolidayList_ReturnsZeroStatistics()
    {
        var summary = _analyzer.Analyze("Mumbai", []);

        Assert.Multiple(() =>
        {
            Assert.That(summary.TotalHolidays, Is.Zero);
            Assert.That(summary.HighestHolidayMonth, Is.Empty);
            Assert.That(summary.HighestHolidayMonthCount, Is.Zero);
            Assert.That(summary.LongWeekendCount, Is.Zero);
            Assert.That(summary.LongestHolidayStretch, Is.Zero);
        });
    }

    [Test]
    public void Analyze_SingleHoliday_ReturnsOneTotalAndNoStretches()
    {
        var holidays = new[]
        {
            CreateHoliday("January", 26, 2001)
        };

        var summary = _analyzer.Analyze("Mumbai", holidays);

        Assert.Multiple(() =>
        {
            Assert.That(summary.TotalHolidays, Is.EqualTo(1));
            Assert.That(summary.HighestHolidayMonth, Is.EqualTo("January"));
            Assert.That(summary.HighestHolidayMonthCount, Is.EqualTo(1));
            Assert.That(summary.LongWeekendCount, Is.Zero);
            Assert.That(summary.LongestHolidayStretch, Is.EqualTo(1));
        });
    }

    [Test]
    public void Analyze_ThreeConsecutiveDays_CountsOneLongWeekend()
    {
        var holidays = new[]
        {
            CreateHoliday("January", 25, 2001),
            CreateHoliday("January", 26, 2001),
            CreateHoliday("January", 27, 2001)
        };

        var summary = _analyzer.Analyze("Mumbai", holidays);

        Assert.Multiple(() =>
        {
            Assert.That(summary.TotalHolidays, Is.EqualTo(3));
            Assert.That(summary.LongWeekendCount, Is.EqualTo(1));
            Assert.That(summary.LongestHolidayStretch, Is.EqualTo(3));
        });
    }

    [Test]
    public void Analyze_FourConsecutiveDays_CountsOneLongWeekend()
    {
        var holidays = new[]
        {
            CreateHoliday("March", 10, 2001),
            CreateHoliday("March", 11, 2001),
            CreateHoliday("March", 12, 2001),
            CreateHoliday("March", 13, 2001)
        };

        var summary = _analyzer.Analyze("Mumbai", holidays);

        Assert.Multiple(() =>
        {
            Assert.That(summary.TotalHolidays, Is.EqualTo(4));
            Assert.That(summary.LongWeekendCount, Is.EqualTo(1));
            Assert.That(summary.LongestHolidayStretch, Is.EqualTo(4));
        });
    }

    [Test]
    public void Analyze_MultipleSeparateLongStretches_CountsEach()
    {
        var holidays = new[]
        {
            // Stretch 1: Jan 25-27 (3 days)
            CreateHoliday("January", 25, 2001),
            CreateHoliday("January", 26, 2001),
            CreateHoliday("January", 27, 2001),
            // Stretch 2: Apr 13-15 (3 days)
            CreateHoliday("April", 13, 2001),
            CreateHoliday("April", 14, 2001),
            CreateHoliday("April", 15, 2001),
            // Stretch 3: Dec 25-28 (4 days)
            CreateHoliday("December", 25, 2001),
            CreateHoliday("December", 26, 2001),
            CreateHoliday("December", 27, 2001),
            CreateHoliday("December", 28, 2001)
        };

        var summary = _analyzer.Analyze("Mumbai", holidays);

        Assert.Multiple(() =>
        {
            Assert.That(summary.TotalHolidays, Is.EqualTo(10));
            Assert.That(summary.LongWeekendCount, Is.EqualTo(3));
            Assert.That(summary.LongestHolidayStretch, Is.EqualTo(4));
        });
    }

    [Test]
    public void Analyze_HighestHolidayCountMonth_ReturnsCorrectMonth()
    {
        var holidays = new[]
        {
            CreateHoliday("January", 1, 2001),
            CreateHoliday("January", 2, 2001),
            CreateHoliday("January", 3, 2001),
            CreateHoliday("February", 1, 2001),
            CreateHoliday("February", 2, 2001),
            CreateHoliday("March", 1, 2001)
        };

        var summary = _analyzer.Analyze("Mumbai", holidays);

        Assert.Multiple(() =>
        {
            Assert.That(summary.HighestHolidayMonth, Is.EqualTo("January"));
            Assert.That(summary.HighestHolidayMonthCount, Is.EqualTo(3));
        });
    }

    [Test]
    public void Analyze_NonConsecutiveDates_NoLongWeekends()
    {
        var holidays = new[]
        {
            CreateHoliday("January", 1, 2001),
            CreateHoliday("January", 5, 2001),
            CreateHoliday("January", 10, 2001),
            CreateHoliday("February", 2, 2001),
            CreateHoliday("March", 15, 2001)
        };

        var summary = _analyzer.Analyze("Mumbai", holidays);

        Assert.Multiple(() =>
        {
            Assert.That(summary.TotalHolidays, Is.EqualTo(5));
            Assert.That(summary.LongWeekendCount, Is.Zero);
            Assert.That(summary.LongestHolidayStretch, Is.EqualTo(1));
        });
    }

    [Test]
    public void Analyze_TwoConsecutiveDays_NotCountedAsLongWeekend()
    {
        var holidays = new[]
        {
            CreateHoliday("January", 25, 2001),
            CreateHoliday("January", 26, 2001)
        };

        var summary = _analyzer.Analyze("Mumbai", holidays);

        Assert.Multiple(() =>
        {
            Assert.That(summary.TotalHolidays, Is.EqualTo(2));
            Assert.That(summary.LongWeekendCount, Is.Zero);
            Assert.That(summary.LongestHolidayStretch, Is.EqualTo(2));
        });
    }

    private static HolidayRecord CreateHoliday(string month, int day, int year) =>
        new()
        {
            Month = month,
            Day = day,
            Occasion = $"Test holiday {month} {day}",
            Year = year,
            Date = new DateOnly(year, GetMonthNumber(month), day)
        };

    private static int GetMonthNumber(string month) =>
        month switch
        {
            "January" => 1,
            "February" => 2,
            "March" => 3,
            "April" => 4,
            "May" => 5,
            "June" => 6,
            "July" => 7,
            "August" => 8,
            "September" => 9,
            "October" => 10,
            "November" => 11,
            "December" => 12,
            _ => throw new ArgumentOutOfRangeException(nameof(month))
        };
}