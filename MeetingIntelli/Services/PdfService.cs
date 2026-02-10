//using MeetingIntelli.Configurations;
//using Microsoft.Extensions.Options;
//using Microsoft.Playwright;

//namespace MeetingIntelli.Services;

//public class PdfService : IPdfService
//{
//    private readonly FrontEndSettings _frontendSettings;
//    private readonly ILogger<PdfService> _logger;
//    private readonly IBrowserPool _browserPool;

//    public PdfService(
//        IOptions<FrontEndSettings> frontendSettings,
//        ILogger<PdfService> logger,
//        IBrowserPool browserPool)
//    {
//        _frontendSettings = frontendSettings?.Value ?? throw new ArgumentNullException(nameof(frontendSettings));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        _browserPool = browserPool ?? throw new ArgumentNullException(nameof(browserPool));
//    }

//    public async Task<byte[]> GeneratePdfFromUrlAsync(
//        string urlPath,
//        int? viewportWidth = null,
//        int? viewportHeight = null,
//        CancellationToken cancellationToken = default)
//    {
//        if (string.IsNullOrWhiteSpace(urlPath))
//        {
//            throw new ArgumentException("URL path cannot be empty", nameof(urlPath));
//        }

//        // if viewport not available use default
//        var width = viewportWidth ?? 1920;
//        var height = viewportHeight ?? 1080;

//        var fullUrl = $"{_frontendSettings.BaseUrl}{urlPath}";

//        _logger.LogInformation("Generating PDF from URL: {Url} with viewport {Width}x{Height}",
//            fullUrl, width, height);

//        try
//        {
//            var browser = await _browserPool.GetBrowserAsync();

//            var context = await browser.NewContextAsync(new()
//            {
//                ViewportSize = new ViewportSize
//                {
//                    Width = width,
//                    Height = height
//                }
//            });

//            var page = await context.NewPageAsync();

//            _logger.LogDebug("Navigating to {Url}", fullUrl);
//            await page.GotoAsync(fullUrl, new()
//            {
//                WaitUntil = WaitUntilState.NetworkIdle,
//                Timeout = 30000
//            });

//            _logger.LogDebug("Waiting for page to be ready for PDF generation");
//            await page.WaitForSelectorAsync("[data-pdf-ready='true']", new()
//            {
//                Timeout = 15000
//            });

//            await page.WaitForTimeoutAsync(1000);

//            _logger.LogDebug("Generating PDF");
//            //var pdf = await page.PdfAsync(new()
//            //{
//            //    Format = "A4",
//            //    PrintBackground = true,
//            //    Margin = new()
//            //    {
//            //        Top = "0.5cm",
//            //        Right = "0.5cm",
//            //        Bottom = "0.5cm",
//            //        Left = "0.5cm"
//            //    }
//            //});

//            var pdf = await page.PdfAsync(new()
//            {

//                PrintBackground = true,
//                Width = $"{width}px",      
//                Height = $"{height}px",   
//                Margin = new()
//                {
//                    Top = "0.5cm",
//                    Right = "0.5cm",
//                    Bottom = "0.5cm",
//                    Left = "0.5cm"
//                }
//            });
//            // Clean up context  but also keep browser alive
//            await context.CloseAsync();

//            _logger.LogInformation("Successfully generated PDF ({Size} bytes) from viewport {Width}x{Height}",
//                pdf.Length, width, height);

//            return pdf;
//        }
//        catch (PlaywrightException ex)
//        {
//            _logger.LogError(ex, "Playwright error generating PDF from {Url}", fullUrl);
//            throw new InvalidOperationException($"Failed to generate PDF: {ex.Message}", ex);
//        }
//        catch (TimeoutException ex)
//        {
//            _logger.LogError(ex, "Timeout generating PDF from {Url}", fullUrl);
//            throw new InvalidOperationException("PDF generation timed out", ex);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Unexpected error generating PDF from {Url}", fullUrl);
//            throw;
//        }
//    }
//}

using MeetingIntelli.Configurations;
using MeetingIntelli.Interface;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using System;

namespace MeetingIntelli.Services;

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
        int? viewportWidth = null,
        int? viewportHeight = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(urlPath))
        {
            throw new ArgumentException("URL path cannot be empty", nameof(urlPath));
        }

        var width = viewportWidth ?? 1920;
        var height = viewportHeight ?? 1080;

        var fullUrl = $"{_frontendSettings.BaseUrl}{urlPath}";

        _logger.LogInformation("Generating PDF from URL: {Url} with viewport {Width}x{Height}",
            fullUrl, width, height);

        try
        {
            var browser = await _browserPool.GetBrowserAsync();

            var context = await browser.NewContextAsync(new()
            {
                ViewportSize = new ViewportSize
                {
                    Width = width,
                    Height = height
                }
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

            // Get full content height (entire scrollable page)
            var scrollHeight = await page.EvaluateAsync<int>(
                "document.documentElement.scrollHeight"
            );

            _logger.LogDebug("Page scroll height: {ScrollHeight}px (viewport was {ViewportHeight}px)",
                scrollHeight, height);

            // Generate PDF with full content height (single continuous page)
            var pdf = await page.PdfAsync(new()
            {
                PrintBackground = true,
                Width = $"{width}px",
                Height = $"{scrollHeight}px",  
                Margin = new()
                {
                    Top = "0.5cm",
                    Right = "0.5cm",
                    Bottom = "0.5cm",
                    Left = "0.5cm"
                },
                PreferCSSPageSize = false
            });

            await context.CloseAsync();

            _logger.LogInformation("Successfully generated PDF ({Size} bytes) - {Width}x{ScrollHeight}px (single page)",
                pdf.Length, width, scrollHeight);

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