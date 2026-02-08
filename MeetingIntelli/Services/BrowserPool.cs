using Microsoft.Playwright;

namespace MeetingIntelli.Services;

public interface IBrowserPool
{
    Task<IBrowser> GetBrowserAsync();
}

public class BrowserPool : IBrowserPool, IAsyncDisposable
{
    private IBrowser? _browser;
    private IPlaywright? _playwright;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<IBrowser> GetBrowserAsync()
    {
        if (_browser == null)
        {
            await _lock.WaitAsync();
            try
            {
                if (_browser == null)
                {
                    _playwright = await Playwright.CreateAsync();
                    _browser = await _playwright.Chromium.LaunchAsync(new()
                    {
                        Headless = true,
                        Args = new[]
                        {
                            "--no-sandbox",
                            "--disable-setuid-sandbox",
                            "--disable-dev-shm-usage"
                        }
                    });
                }
            }
            finally
            {
                _lock.Release();
            }
        }
        return _browser;
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }

        if (_playwright != null)
        {
            _playwright.Dispose();
            _playwright = null;
        }

        _lock.Dispose();
    }
}