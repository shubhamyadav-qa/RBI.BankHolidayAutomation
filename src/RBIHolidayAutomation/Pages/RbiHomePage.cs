using Microsoft.Playwright;

namespace RBIHolidayAutomation.Pages;

/// <summary>
/// Page Object for the RBI home page.
/// </summary>
public sealed class RbiHomePage
{
    private readonly IPage _page;

    private const string HomeUrl = "https://www.rbi.org.in/";

    public RbiHomePage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Opens the RBI home page and waits for it to finish loading.
    /// </summary>
    public async Task OpenAsync()
    {
        await _page.GotoAsync(HomeUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 60_000
        });
        await DismissLanguageDialogAsync();
    }

    /// <summary>
    /// Dismisses the language-selection dialog that appears on page load.
    /// </summary>
    private async Task DismissLanguageDialogAsync()
    {
        var dialog = _page.Locator(".ui-dialog");
        if (await dialog.CountAsync() > 0 && await dialog.IsVisibleAsync())
        {
            await _page.Keyboard.PressAsync("Escape");
            await dialog.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = 5_000
            });
        }
    }

    /// <summary>
    /// Scrolls to the bottom of the page so footer links become visible.
    /// </summary>
    public async Task ScrollToBottomAsync()
    {
        await _page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
        // Wait for the footer link to become visible rather than using a fixed timeout.
        await _page.GetByRole(AriaRole.Link, new() { Name = "Bank Holidays" }).WaitForAsync(
            new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    /// <summary>
    /// Clicks the "Bank Holidays" link in the footer.
    /// </summary>
    public async Task ClickBankHolidaysAsync()
    {
        var bankHolidaysLink = _page.GetByRole(AriaRole.Link, new() { Name = "Bank Holidays" });
        await bankHolidaysLink.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}