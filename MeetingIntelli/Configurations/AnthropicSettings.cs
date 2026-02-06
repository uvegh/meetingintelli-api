namespace MeetingIntelli.Configurations;

    public class AnthropicSettings
    {
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.anthropic.com";
    public string Model { get; set; } = "claude-sonnet-4-20250514";

    public int MaxTokens { get; set; } = 2048;
    public int TimeoutSeconds { get; set; } = 60;
}

