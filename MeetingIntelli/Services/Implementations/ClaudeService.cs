using MeetingIntelli.Configurations;
using MeetingIntelli.Services.Interface;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MeetingIntelli.Services.Implementations;

public class ClaudeService : IClaudeService
{
    private static readonly JsonSerializerOptions CachedJsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _httpClient;
    private readonly AnthropicSettings _settings;
    private readonly ILogger<ClaudeService> _logger;

    public ClaudeService(
        HttpClient httpClient,
        IOptions<AnthropicSettings> settings,
        ILogger<ClaudeService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<string> GetCompletionAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be empty", nameof(prompt));
        }

        try
        {
            _logger.LogInformation("Sending request to Claude API");

            var request = new
            {
                model = _settings.Model,
                max_tokens = _settings.MaxTokens,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                "/v1/messages",
                content,
                cancellationToken
            );

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var claudeResponse = JsonSerializer.Deserialize<ClaudeApiResponse>(
                responseContent,
                CachedJsonOptions
            );

            var text = claudeResponse?.Content?.FirstOrDefault()?.Text ?? string.Empty;

            _logger.LogInformation("Successfully received response from Claude API");

            return text;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Claude API");
            throw new InvalidOperationException("Failed to communicate with Claude API", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Claude API request timed out");
            throw new InvalidOperationException("Claude API request timed out", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Claude API");
            throw;
        }
    }

    private class ClaudeApiResponse
    {
        public List<ContentBlock>? Content { get; set; }
    }

    private class ContentBlock
    {
        public string Type { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}