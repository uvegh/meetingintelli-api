namespace MeetingIntelli.Interface;

public interface IClaudeService
{
    /// <summary>
    /// Generates a completion response for the specified prompt asynchronously.
    /// </summary>
    /// <param name="prompt">The input text prompt for which a completion response is requested. Cannot be null or empty.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated completion as a
    /// string.</returns>

    Task<string> GetCompletionAsync(string prompt, CancellationToken ct=default);
}
