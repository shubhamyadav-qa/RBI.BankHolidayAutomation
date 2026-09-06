using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace RBIHolidayAutomation.Utilities;

/// <summary>
/// Creates and configures Playwright browser instances.
/// </summary>
public sealed class BrowserFactory
{
    private readonly bool _headless;

    public BrowserFactory(IConfiguration configuration)
    {
        _headless = configuration.GetValue("Browser:Headless", true);
    }

    /// <summary>
    /// Launches a new Chromium browser instance.
    /// </summary>
    public async Task<IBrowser> LaunchAsync()
    {
        var playwright = await Playwright.CreateAsync();
        return await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = _headless
        });
    }
}