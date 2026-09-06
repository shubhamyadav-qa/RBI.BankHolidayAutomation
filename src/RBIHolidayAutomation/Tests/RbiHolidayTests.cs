using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using RBIHolidayAutomation.Models;
using RBIHolidayAutomation.Pages;
using RBIHolidayAutomation.Services;
using RBIHolidayAutomation.Utilities;

namespace RBIHolidayAutomation.Tests;

[TestFixture]
public sealed class RbiHolidayTests
{
    private const int TargetYear = 2001;
    private const string AllMonths = "All Months";
    private const string TestResultsDirectory = "TestResults";

    private static string ReportPath =>
        Path.Combine(FindSolutionRoot(), "Reports", "rbi_2001_holidays.txt");

    private static readonly string[] RegionalOffices = ["Mumbai", "Srinagar"];

    private IConfiguration _configuration = null!;
    private BrowserFactory _browserFactory = null!;
    private HolidayAnalyzer _analyzer = null!;
    private HolidayReportWriter _reportWriter = null!;

    [SetUp]
    public void SetUp()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        _browserFactory = new BrowserFactory(_configuration);
        _analyzer = new HolidayAnalyzer();
        _reportWriter = new HolidayReportWriter();
    }

    [Test]
    public async Task ExtractAndAnalyzeRbiHolidays_ForMumbaiAndSrinagar_GeneratesReport()
    {
        await using var browser = await _browserFactory.LaunchAsync();
        var page = await browser.NewPageAsync();

        try
        {
            TestContext.Progress.WriteLine("Opening RBI website");
            var homePage = new RbiHomePage(page);
            await homePage.OpenAsync();
            await homePage.ScrollToBottomAsync();

            TestContext.Progress.WriteLine("Navigating to Bank Holidays");
            await homePage.ClickBankHolidaysAsync();

            var bankHolidaysPage = new BankHolidaysPage(page);
            var sections = new List<(string RegionalOffice, IReadOnlyList<HolidayRecord> Holidays, HolidaySummary Summary)>();

            foreach (var regionalOffice in RegionalOffices)
            {
                TestContext.Progress.WriteLine($"Selecting regional office: {regionalOffice}");
                TestContext.Progress.WriteLine($"Selecting month: {AllMonths}");
                TestContext.Progress.WriteLine($"Selecting year: {TargetYear}");

                await bankHolidaysPage.SearchHolidaysAsync(regionalOffice, AllMonths, TargetYear.ToString());

                TestContext.Progress.WriteLine("Extracting holiday records");
                var holidays = await bankHolidaysPage.ExtractHolidaysAsync(TargetYear);

                Assert.That(holidays, Is.Not.Empty,
                    $"No holidays were extracted for {regionalOffice}. The results table may not have loaded.");

                TestContext.Progress.WriteLine($"Extracted {holidays.Count} holidays for {regionalOffice}");

                TestContext.Progress.WriteLine("Calculating holiday statistics");
                var summary = _analyzer.Analyze(regionalOffice, holidays);

                Assert.That(summary.TotalHolidays, Is.GreaterThan(0),
                    $"Total holidays must be greater than zero for {regionalOffice}.");
                Assert.That(summary.HighestHolidayMonth, Is.Not.Null.And.Not.Empty,
                    $"Highest holiday month must not be empty for {regionalOffice}.");
                Assert.That(summary.HighestHolidayMonthCount, Is.GreaterThan(0),
                    $"Highest holiday month count must be greater than zero for {regionalOffice}.");

                sections.Add((regionalOffice, holidays, summary));
            }

            TestContext.Progress.WriteLine("Generating report");
            await _reportWriter.WriteReportAsync(ReportPath, TargetYear, sections);

            Assert.That(File.Exists(ReportPath), Is.True, $"Report file was not created at {ReportPath}.");

            var reportContent = await File.ReadAllTextAsync(ReportPath);
            Assert.That(reportContent, Does.Contain("REGIONAL OFFICE: MUMBAI"),
                "Report must contain the MUMBAI section.");
            Assert.That(reportContent, Does.Contain("REGIONAL OFFICE: SRINAGAR"),
                "Report must contain the SRINAGAR section.");
            Assert.That(reportContent, Does.Contain("Total holidays:"),
                "Report must contain the total holidays summary.");
            Assert.That(reportContent, Does.Contain("Month with highest number of holidays:"),
                "Report must contain the highest holiday month summary.");
            Assert.That(reportContent, Does.Contain("Long weekends"),
                "Report must contain the long weekend summary.");
            Assert.That(reportContent, Does.Contain("Longest continuous holiday stretch:"),
                "Report must contain the longest stretch summary.");

            TestContext.Progress.WriteLine($"Report generated at {Path.GetFullPath(ReportPath)}");
        }
        catch (Exception ex)
        {
            await CaptureDiagnosticsAsync(page, ex);
            throw;
        }
    }

    /// <summary>
    /// Captures a screenshot and diagnostic information when the test fails.
    /// </summary>
    private static async Task CaptureDiagnosticsAsync(IPage page, Exception exception)
    {
        try
        {
            var diagnosticsDir = Path.Combine(FindSolutionRoot(), TestResultsDirectory);
            Directory.CreateDirectory(diagnosticsDir);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var screenshotPath = Path.Combine(diagnosticsDir, $"failure_{timestamp}.png");
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = true
            });

            var infoPath = Path.Combine(diagnosticsDir, $"failure_{timestamp}.txt");
            await File.WriteAllTextAsync(infoPath,
                $"URL: {page.Url}\n" +
                $"Error: {exception}\n" +
                $"Screenshot: {screenshotPath}\n");

            TestContext.Progress.WriteLine($"Diagnostics saved to {diagnosticsDir}");
        }
        catch (Exception diagEx)
        {
            TestContext.Progress.WriteLine($"Failed to capture diagnostics: {diagEx.Message}");
        }
    }

    /// <summary>
    /// Walks up from the test output directory to find the solution root.
    /// </summary>
    private static string FindSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RBIHolidayAutomation.sln")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate the solution root directory.");
    }
}