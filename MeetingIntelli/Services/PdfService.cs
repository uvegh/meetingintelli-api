using MeetingIntelli.Configurations;
using MeetingIntelli.Interface;
using MeetingIntelli.Services;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

public class PdfService : IPdfService
{
    private readonly FrontEndSettings _frontendSettings;
    private readonly ILogger<PdfService> _logger;
    private readonly IBrowserPool _browserPool;

    public PdfService(
        IOptions<FrontEndSettings> frontendSettings,
        ILogger<PdfService> logger,
        IBrowserPool browserPool)
    {
        _frontendSettings = frontendSettings?.Value ?? throw new ArgumentNullException(nameof(frontendSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _browserPool = browserPool ?? throw new ArgumentNullException(nameof(browserPool));
    }

    public async Task<byte[]> GeneratePdfFromUrlAsync(
        string urlPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(urlPath))
        {
            throw new ArgumentException("URL path cannot be empty", nameof(urlPath));
        }


        var fullUrl = $"{_frontendSettings.BaseUrl}{urlPath}";

        _logger.LogInformation("Generating PDF from URL: {Url}", fullUrl);

        try
        {

            var browser = await _browserPool.GetBrowserAsync();

            var context = await browser.NewContextAsync(new()
            {
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
            });

            var page = await context.NewPageAsync();

            _logger.LogDebug("Navigating to {Url}", fullUrl);
            await page.GotoAsync(fullUrl, new()
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            _logger.LogDebug("Waiting for page to be ready for PDF generation");
            await page.WaitForSelectorAsync("[data-pdf-ready='true']", new()
            {
                Timeout = 15000
            });

            await page.WaitForTimeoutAsync(1000);

            _logger.LogDebug("Generating PDF");
            var pdf = await page.PdfAsync(new()
            {
                Format = "A4",
                PrintBackground = true,
                Margin = new()
                {
                    Top = "1cm",
                    Right = "1cm",
                    Bottom = "1cm",
                    Left = "1cm"
                }
            });

            // Clean up context (but keep browser alive)
            await context.CloseAsync();

            _logger.LogInformation("Successfully generated PDF ({Size} bytes)", pdf.Length);
            return pdf;
        }
        catch (PlaywrightException ex)
        {
            _logger.LogError(ex, "Playwright error generating PDF from {Url}", fullUrl);
            throw new InvalidOperationException($"Failed to generate PDF: {ex.Message}", ex);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout generating PDF from {Url}", fullUrl);
            throw new InvalidOperationException("PDF generation timed out", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating PDF from {Url}", fullUrl);
            throw;
        }
    }
}